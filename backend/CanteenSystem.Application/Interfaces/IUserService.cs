using CanteenSystem.Application.DTOs.User;

namespace CanteenSystem.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(Guid userId);
    Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    
    // FIX: Use Stream instead of IFormFile
    Task<UserProfileResponse> UploadProfileImageAsync(Guid userId, Stream fileStream, string fileName, string contentType);
}