using CanteenSystem.Application.Common;
using CanteenSystem.Application.DTOs.Auth;
using CanteenSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CanteenSystem.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
            var (response, refreshToken) = await _authService.RegisterAsync(request);
            SetRefreshTokenCookie(refreshToken);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Registration successful."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
            var (response, refreshToken) = await _authService.LoginAsync(request);
            SetRefreshTokenCookie(refreshToken);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful."));
    }

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // JavaScript cannot read this
            Secure = Request.IsHttps,   // Only sent over HTTPS
            SameSite = SameSiteMode.Strict, // Prevents CSRF attacks
            Expires = DateTime.UtcNow.AddDays(7) // Should match your appsettings config
        };

        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
            }

            // Delete the cookie from the browser
        Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict
            });

        return Ok(ApiResponse<object>.SuccessResponse(null!, "Logged out successfully."));
    }
   
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh()
    {
        var oldRefreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(oldRefreshToken))
        {
            return Unauthorized(ApiResponse<AuthResponse>.FailureResponse("No refresh token found. Please log in."));
        }

        try
        {
            var (response, newRefreshToken) = await _authService.RefreshTokenAsync(oldRefreshToken);

            SetRefreshTokenCookie(newRefreshToken);

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Token refreshed successfully."));
        }
        catch (Exception ex)
        {
            // If anything fails (expired, stolen, invalid), clear the cookie to force a clean login
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict
            });

            return Unauthorized(ApiResponse<AuthResponse>.FailureResponse(ex.Message));
        }
    }
}
