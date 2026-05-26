using System.Net;
using CanteenSystem.Application.Common.Exceptions;
using CanteenSystem.Application.Common.Helpers;
using CanteenSystem.Application.DTOs.Auth;
using CanteenSystem.Application.Interfaces;
using CanteenSystem.Domain.Entities;
using CanteenSystem.Domain.Enums;
using CanteenSystem.Domain.Interfaces;

namespace CanteenSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public AuthService(
        IGenericRepository<User> userRepository,
        IGenericRepository<RefreshToken> refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IOtpService otpService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetFirstOrDefaultAsync(
            u => u.Email.ToLower() == request.Email.ToLower());

        if (existing != null)
        {
            throw new AppException(
                "An account with this email already exists.",
                HttpStatusCode.Conflict);
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User,
            IsActive = false,
            EmailVerifiedAt = null
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var otpCode = await _otpService.GenerateAndSaveOtpAsync(
            user.Id,
            OtpType.EmailVerification);

        await _emailService.SendEmailVerificationOtpAsync(
            user.Email,
            user.Name,
            otpCode,
            expiryMinutes: 10);

        return new RegisterResponse
        {
            Message = "Registration successful. Please check your email for the verification code.",
            Email = user.Email
        };
    }

    public async Task<(AuthResponse response, string refreshToken)> VerifyEmailAsync(
        VerifyEmailRequest request,
        string? ipAddress = null)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(
            u => u.Email.ToLower() == request.Email.ToLower())
            ?? throw new AppException(
                "User not found.",
                HttpStatusCode.NotFound);

        if (user.EmailVerifiedAt != null)
        {
            throw new AppException(
                "Email is already verified. Please log in.",
                HttpStatusCode.UnprocessableEntity);
        }

        var isValid = await _otpService.VerifyOtpAsync(
            user.Id,
            OtpType.EmailVerification,
            request.OtpCode);

        if (!isValid)
        {
            throw new AppException(
                "Invalid or expired OTP. Please request a new code.",
                HttpStatusCode.UnprocessableEntity);
        }

        user.EmailVerifiedAt = DateTime.UtcNow;
        user.IsActive = true;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        var refreshToken = await GenerateAndSaveRefreshTokenAsync(
            user.Id,
            ipAddress: ipAddress);

        return (ToAuthResponse(user, accessToken), refreshToken);
    }

    public async Task ResendVerificationOtpAsync(ResendOtpRequest request)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(
            u => u.Email.ToLower() == request.Email.ToLower());

        // Silent return for security
        if (user == null || user.EmailVerifiedAt != null)
            return;

        var otpCode = await _otpService.GenerateAndSaveOtpAsync(
            user.Id,
            OtpType.EmailVerification);

        await _emailService.SendEmailVerificationOtpAsync(
            user.Email,
            user.Name,
            otpCode,
            expiryMinutes: 10);
    }

    public async Task<(AuthResponse response, string refreshToken)> LoginAsync(
        LoginRequest request,
        string? ipAddress = null)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(
            u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new AppException(
                "Invalid email or password.",
                HttpStatusCode.Unauthorized);
        }

        if (user.EmailVerifiedAt == null)
        {
            throw new AppException(
                "Please verify your email before logging in.",
                HttpStatusCode.Unauthorized);
        }

        if (!user.IsActive)
        {
            throw new AppException(
                "Your account has been deactivated. Please contact support.",
                HttpStatusCode.Forbidden);
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        var refreshToken = await GenerateAndSaveRefreshTokenAsync(
            user.Id,
            ipAddress: ipAddress);

        return (ToAuthResponse(user, accessToken), refreshToken);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(
            u => u.Email.ToLower() == request.Email.ToLower());

        // Silent return for security
        if (user == null)
            return;

        if (user.EmailVerifiedAt == null)
        {
            throw new AppException(
                "Please verify your email first before resetting your password.",
                HttpStatusCode.UnprocessableEntity);
        }

        var otpCode = await _otpService.GenerateAndSaveOtpAsync(
            user.Id,
            OtpType.PasswordReset);

        await _emailService.SendPasswordResetOtpAsync(
            user.Email,
            user.Name,
            otpCode,
            expiryMinutes: 10);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(
            u => u.Email.ToLower() == request.Email.ToLower())
            ?? throw new AppException(
                "User not found.",
                HttpStatusCode.NotFound);

        var isValid = await _otpService.VerifyOtpAsync(
            user.Id,
            OtpType.PasswordReset,
            request.OtpCode);

        if (!isValid)
        {
            throw new AppException(
                "Invalid or expired OTP.",
                HttpStatusCode.UnprocessableEntity);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _userRepository.UpdateAsync(user);

        var activeTokens = (await _refreshTokenRepository.GetWhereAsync(
            t => t.UserId == user.Id && !t.IsRevoked)).ToList();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "PasswordReset";

            await _refreshTokenRepository.UpdateAsync(token);
        }

        await _userRepository.SaveChangesAsync();
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var hashed = CryptoHelper.HashWithSha256(refreshToken);

        var tokenEntity = await _refreshTokenRepository.GetFirstOrDefaultAsync(
            t => t.TokenHash == hashed);

        if (tokenEntity == null || tokenEntity.IsRevoked)
            return;

        tokenEntity.IsRevoked = true;
        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.RevokedReason = "Logout";

        await _refreshTokenRepository.UpdateAsync(tokenEntity);
        await _refreshTokenRepository.SaveChangesAsync();
    }

    public async Task<(AuthResponse response, string newRefreshToken)> RefreshTokenAsync(
        string oldRefreshToken,
        string? ipAddress = null)
    {
        var hashed = CryptoHelper.HashWithSha256(oldRefreshToken);

        var tokenEntity = await _refreshTokenRepository.GetFirstOrDefaultAsync(
            t => t.TokenHash == hashed)
            ?? throw new AppException(
                "Invalid refresh token.",
                HttpStatusCode.Unauthorized);

        if (tokenEntity.IsUsed || tokenEntity.IsRevoked)
        {
            await RevokeTokenFamilyAsync(
                tokenEntity.Family,
                reason: "TokenReuse");

            throw new AppException(
                "Security alert: token already used. Please log in again.",
                HttpStatusCode.Unauthorized);
        }

        if (tokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            tokenEntity.IsUsed = true;
            tokenEntity.RevokedAt = DateTime.UtcNow;
            tokenEntity.RevokedReason = "Expired";

            await _refreshTokenRepository.UpdateAsync(tokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            throw new AppException(
                "Refresh token expired. Please log in again.",
                HttpStatusCode.Unauthorized);
        }

        tokenEntity.IsUsed = true;
        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.RevokedReason = "Rotated";

        await _refreshTokenRepository.UpdateAsync(tokenEntity);

        var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);

        if (user == null || !user.IsActive)
        {
            throw new AppException(
                "User account is inactive or not found.",
                HttpStatusCode.Unauthorized);
        }

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);

        var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(
            user.Id,
            existingFamily: tokenEntity.Family,
            ipAddress: ipAddress);

        return (ToAuthResponse(user, newAccessToken), newRefreshToken);
    }

    // ─────────────────────────────────────────────────────────────

    private async Task RevokeTokenFamilyAsync(
        string family,
        string reason)
    {
        var familyTokens = (await _refreshTokenRepository.GetWhereAsync(
            t => t.Family == family && !t.IsRevoked)).ToList();

        foreach (var token in familyTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = reason;

            await _refreshTokenRepository.UpdateAsync(token);
        }

        await _refreshTokenRepository.SaveChangesAsync();
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(
        Guid userId,
        string? existingFamily = null,
        string? ipAddress = null)
    {
        var (rawToken, expiresAt) =
            _jwtTokenGenerator.GenerateRefreshToken();

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = CryptoHelper.HashWithSha256(rawToken),
            Family = existingFamily ?? Guid.NewGuid().ToString(),
            ExpiresAt = expiresAt,
            IsUsed = false,
            IsRevoked = false,
            IpAddress = ipAddress
        };

        await _refreshTokenRepository.AddAsync(entity);
        await _refreshTokenRepository.SaveChangesAsync();

        return rawToken;
    }

    private static AuthResponse ToAuthResponse(
        User user,
        string accessToken)
    {
        return new AuthResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = accessToken
        };
    }
}