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
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request)
    {
        // FIX: Now we just get the response back, no tokens.
        var response = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<RegisterResponse>.SuccessResponse(response,
            "Registration successful. Check your email for the verification code."));
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var ipAddress = GetClientIpAddress();
        
        // This is where tokens are finally issued!
        var (response, refreshToken) = await _authService.VerifyEmailAsync(request, ipAddress);
        SetRefreshTokenCookie(refreshToken);
        
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Email verified. Welcome!"));
    }

    [HttpPost("resend-verification-otp")]
    public async Task<ActionResult<ApiResponse<object>>> ResendVerificationOtp([FromBody] ResendOtpRequest request)
    {
        await _authService.ResendVerificationOtpAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(null!,
            "If your email is registered and unverified, a new code has been sent."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var ipAddress = GetClientIpAddress();
        var (response, refreshToken) = await _authService.LoginAsync(request, ipAddress);
        SetRefreshTokenCookie(refreshToken);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful."));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(null!,
            "If your email is registered, a password reset code has been sent."));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse<object>.SuccessResponse(null!,
            "Password reset successfully. Please log in with your new password."));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshToken))
            await _authService.LogoutAsync(refreshToken);

        DeleteRefreshTokenCookie();
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Logged out successfully."));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh()
    {
        var oldRefreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(oldRefreshToken))
            return Unauthorized(ApiResponse<AuthResponse>.FailureResponse("No refresh token found. Please log in."));

        try
        {
            var ipAddress = GetClientIpAddress();
            var (response, newRefreshToken) = await _authService.RefreshTokenAsync(oldRefreshToken, ipAddress);
            SetRefreshTokenCookie(newRefreshToken);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Token refreshed."));
        }
        catch
        {
            DeleteRefreshTokenCookie();
            return Unauthorized(ApiResponse<AuthResponse>.FailureResponse("Session expired. Please log in again."));
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string token)
    {
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly  = true,
            Secure    = Request.IsHttps,
            SameSite  = SameSiteMode.Strict,
            Expires   = DateTime.UtcNow.AddDays(7)
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure   = Request.IsHttps,
            SameSite = SameSiteMode.Strict
        });
    }

    private string? GetClientIpAddress()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}