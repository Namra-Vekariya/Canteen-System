using System.Security.Cryptography;
using System.Text;
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

    public AuthService(
        IGenericRepository<User> userRepository, 
        IGenericRepository<RefreshToken> refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<(AuthResponse response, string refreshToken)> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetFirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (existingUser != null) throw new Exception("Email is already registered.");

        var newUser = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User 
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(newUser);
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(newUser.Id);

        var response = new AuthResponse
        {
            Id = newUser.Id,
            Name = newUser.Name,
            Email = newUser.Email,
            Role = newUser.Role.ToString(),
            AccessToken = accessToken
        };

        return (response, refreshToken);
    }

    public async Task<(AuthResponse response, string refreshToken)> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
        if (user == null || !user.IsActive) throw new Exception("Invalid credentials.");

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid) throw new Exception("Invalid credentials.");

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

        var response = new AuthResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = accessToken
        };

        return (response, refreshToken);
    }

    // LOGOUT 
    public async Task LogoutAsync(string refreshToken)
    {
        var hashedToken = HashTokenWithSha256(refreshToken);
        
        // Find the token in the DB
        var tokenEntity = await _refreshTokenRepository.GetFirstOrDefaultAsync(t => t.TokenHash == hashedToken);
        
        if (tokenEntity != null && !tokenEntity.IsRevoked)
        {
            tokenEntity.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(tokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();
        }
    }

public async Task<(AuthResponse response, string newRefreshToken)> RefreshTokenAsync(string oldRefreshToken)
    {
        var hashedToken = HashTokenWithSha256(oldRefreshToken);

        // Find the token in the database, including the associated User
        // or just fetch the user separately using the tokenEntity.UserId.
        var tokenEntity = await _refreshTokenRepository.GetFirstOrDefaultAsync(t => t.TokenHash == hashedToken);

        if (tokenEntity == null)
        {
            throw new Exception("Invalid refresh token.");
        }

        // TOKEN REUSE DETECTION (Compromise Defense)
        if (tokenEntity.IsUsed || tokenEntity.IsRevoked)
        {
            // Revoke the entire family to lock out the potential attacker.
            await RevokeTokenFamilyAsync(tokenEntity.Family);
            throw new Exception("Security breach detected. Please log in again.");
        }

        // Check Expiration
        if (tokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            tokenEntity.IsUsed = true;
            await _refreshTokenRepository.UpdateAsync(tokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();
            throw new Exception("Refresh token expired. Please log in again.");
        }

        // Mark the current token as used
        tokenEntity.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(tokenEntity);

        // Fetch the User
        var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);
        if (user == null || !user.IsActive)
        {
            throw new Exception("User account is inactive or missing.");
        }

        // Generate New Tokens
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id, tokenEntity.Family); // Keep the same family!

        var response = new AuthResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = newAccessToken
        };

        return (response, newRefreshToken);
    }

    // Helper method to revoke all tokens in a family
    private async Task RevokeTokenFamilyAsync(string family)
    {
        var familyTokens = (await _refreshTokenRepository.GetAllAsync())
            .Where(t => t.Family == family && !t.IsRevoked)
            .ToList();

        foreach (var token in familyTokens)
        {
            token.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(token);
        }

        await _refreshTokenRepository.SaveChangesAsync();
    }
    
    private async Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId ,string? existingFamily = null)
    {
        var (token, expiresAt) = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = userId,
            TokenHash = HashTokenWithSha256(token), // Using SHA-256 allows us to look it up later
            Family = existingFamily ?? Guid.NewGuid().ToString(),
            ExpiresAt = expiresAt,
            IsUsed = false,
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _refreshTokenRepository.SaveChangesAsync();

        return token;
    }

    // Helper to securely hash the token for DB storage and lookups
    private static string HashTokenWithSha256(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}