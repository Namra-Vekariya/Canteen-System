using System.Security.Claims;
using CanteenSystem.Application.Common;
using CanteenSystem.Application.DTOs.User;
using CanteenSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanteenSystem.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize] // Every route here requires the user to be logged in
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _userService.GetProfileAsync(userId);
       return Ok(
            ApiResponse<UserProfileResponse>.SuccessResponse(
                profile,
                "Profile fetched successfully"
            )
        );
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        var updatedProfile = await _userService.UpdateProfileAsync(userId, request);
        return Ok(
            ApiResponse<UserProfileResponse>.SuccessResponse(
                updatedProfile,
                "Profile updated successfully"
            )
        );
    }

    [HttpPut("profile-image")]
    public async Task<IActionResult> UploadProfileImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest(new { message = "No image provided." });

        var userId = GetUserId();

        // Extract the stream and pass it to our pure Application layer service
        using var stream = image.OpenReadStream();
        var updatedProfile = await _userService.UploadProfileImageAsync(userId, stream, image.FileName, image.ContentType);

        return Ok(
            ApiResponse<UserProfileResponse>.SuccessResponse(
                updatedProfile,
                "Profile image uploaded successfully"
            )
        );
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token.");

        return userId;
    }
}