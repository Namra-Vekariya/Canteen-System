namespace CanteenSystem.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationOtpAsync(string toEmail, string userName, string otpCode, int expiryMinutes);
    Task SendPasswordResetOtpAsync(string toEmail, string userName, string otpCode, int expiryMinutes);
}