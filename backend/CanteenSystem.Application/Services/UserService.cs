using System.Net;
using CanteenSystem.Application.Common.Exceptions;
using CanteenSystem.Application.DTOs.User;
using CanteenSystem.Application.Interfaces;
using CanteenSystem.Domain.Interfaces;
using UserEntity = CanteenSystem.Domain.Entities.User;

namespace CanteenSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IGenericRepository<UserEntity> _userRepository;
    private readonly IMediaService _mediaService;

    public UserService(IGenericRepository<UserEntity> userRepository, IMediaService mediaService)
    {
        _userRepository = userRepository;
        _mediaService = mediaService;
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
            ?? throw new AppException("User not found.", HttpStatusCode.NotFound);

        return MapToResponse(user);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
            ?? throw new AppException("User not found.", HttpStatusCode.NotFound);

        user.Name = request.Name.Trim();
        user.Phone = request.Phone?.Trim();

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return MapToResponse(user);
    }

    // FIXED: Using Stream instead of IFormFile so it matches the interface perfectly
    public async Task<UserProfileResponse> UploadProfileImageAsync(Guid userId, Stream fileStream, string fileName, string contentType)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
            ?? throw new AppException("User not found.", HttpStatusCode.NotFound);

        // 1. Delete old image if it exists
        if (!string.IsNullOrEmpty(user.ProfileImagePublicId))
        {
            await _mediaService.DeleteAsync(user.ProfileImagePublicId, "image/jpeg"); 
        }

        // 2. Upload new image using your amazing IMediaService
        var uploadResult = await _mediaService.UploadAsync(
            fileStream, 
            fileName, 
            contentType, 
            "Profiles"
        );

        // 3. Save to database
        user.ProfileImageUrl = uploadResult.Url;
        user.ProfileImagePublicId = uploadResult.PublicId;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return MapToResponse(user);
    }

    private static UserProfileResponse MapToResponse(UserEntity user)
    {
        return new UserProfileResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            ProfileImageUrl = user.ProfileImageUrl,
            CreatedAt = user.CreatedAt
        };
    }
}