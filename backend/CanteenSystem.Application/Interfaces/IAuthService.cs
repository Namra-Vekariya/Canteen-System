using CanteenSystem.Application.DTOs.Auth;

namespace CanteenSystem.Application.Interfaces;

public interface IAuthService
{
    // Registration flow (2-step: register → verify email)
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<(AuthResponse response, string refreshToken)> VerifyEmailAsync(VerifyEmailRequest request, string? ipAddress = null);
    Task ResendVerificationOtpAsync(ResendOtpRequest request);

    // Login
    Task<(AuthResponse response, string refreshToken)> LoginAsync(LoginRequest request, string? ipAddress = null);

    // Password reset flow (2-step: forgot → reset)
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);

    // Token management
    Task LogoutAsync(string refreshToken);
    Task<(AuthResponse response, string newRefreshToken)> RefreshTokenAsync(string oldRefreshToken, string? ipAddress = null);
}