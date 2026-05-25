using CanteenSystem.Application.Common.Helpers;
using CanteenSystem.Application.Interfaces;
using CanteenSystem.Domain.Entities;
using CanteenSystem.Domain.Enums;
using CanteenSystem.Domain.Interfaces;

namespace CanteenSystem.Application.Services;

public class OtpService : IOtpService
{
    private readonly IGenericRepository<Otp> _otpRepository;
    private const int OtpExpiryMinutes = 10;
    private const int MaxAttempts = 5;

    public OtpService(IGenericRepository<Otp> otpRepository)
    {
        _otpRepository = otpRepository;
    }

    public async Task<string> GenerateAndSaveOtpAsync(Guid userId, OtpType type)
    {
        await InvalidateExistingOtpsAsync(userId, type);

        var rawOtp = Random.Shared.Next(100_000, 1_000_000).ToString(); // 6 digits

        var otpEntity = new Otp
        {
            UserId   = userId,
            Type     = type,
            OtpHash  = CryptoHelper.HashWithSha256(rawOtp),
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            IsUsed   = false,
            Attempts = 0
        };

        await _otpRepository.AddAsync(otpEntity);
        await _otpRepository.SaveChangesAsync();

        return rawOtp;
    }

    public async Task<bool> VerifyOtpAsync(Guid userId, OtpType type, string rawOtpCode)
    {
        var otpEntity = await _otpRepository.GetFirstOrDefaultAsync(
            o => o.UserId == userId && o.Type == type && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);

        if (otpEntity == null) return false;

        var hashedInput = CryptoHelper.HashWithSha256(rawOtpCode);

        if (otpEntity.OtpHash != hashedInput)
        {
            otpEntity.Attempts++;
            if (otpEntity.Attempts >= MaxAttempts) otpEntity.IsUsed = true;
            await _otpRepository.UpdateAsync(otpEntity);
            await _otpRepository.SaveChangesAsync();
            return false;
        }

        otpEntity.IsUsed = true;
        otpEntity.UsedAt = DateTime.UtcNow;
        await _otpRepository.UpdateAsync(otpEntity);
        await _otpRepository.SaveChangesAsync();

        return true;
    }

    private async Task InvalidateExistingOtpsAsync(Guid userId, OtpType type)
    {
        var activeOtps = (await _otpRepository.GetWhereAsync(
            o => o.UserId == userId && o.Type == type && !o.IsUsed)).ToList();

        foreach (var otp in activeOtps)
        {
            otp.IsUsed = true;
            await _otpRepository.UpdateAsync(otp);
        }

        if (activeOtps.Any()) await _otpRepository.SaveChangesAsync();
    }
}