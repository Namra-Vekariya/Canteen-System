using CanteenSystem.Application.DTOs.Auth;

namespace CanteenSystem.Application.Interfaces;

public interface IAuthService
{
    Task<(AuthResponse response, string refreshToken)> RegisterAsync(RegisterRequest request);
    Task<(AuthResponse response, string refreshToken)> LoginAsync(LoginRequest request);
    Task LogoutAsync(string refreshToken);
    Task<(AuthResponse response, string newRefreshToken)> RefreshTokenAsync(string oldRefreshToken);
}