using CanteenSystem.Domain.Enums;

namespace CanteenSystem.Application.Interfaces;

public interface IOtpService
{
    Task<string> GenerateAndSaveOtpAsync(Guid userId, OtpType type);
    Task<bool> VerifyOtpAsync(Guid userId, OtpType type, string rawOtpCode);
}