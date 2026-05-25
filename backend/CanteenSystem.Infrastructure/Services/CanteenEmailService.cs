using CanteenSystem.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CanteenSystem.Infrastructure.Services;

public class CanteenEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CanteenEmailService> _logger;

    public CanteenEmailService(IConfiguration configuration, ILogger<CanteenEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailVerificationOtpAsync(string toEmail, string userName, string otpCode, int expiryMinutes)
    {
        var subject = "Verify Your Canteen System Account";
        var body = BuildOtpEmailTemplate(
            userName: userName,
            otpCode: otpCode,
            expiryMinutes: expiryMinutes,
            heading: "Verify Your Email",
            subtext: "Use the code below to verify your Canteen System account.",
            accentColor: "#22c55e"
        );

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string userName, string otpCode, int expiryMinutes)
    {
        var subject = "Reset Your Canteen System Password";
        var body = BuildOtpEmailTemplate(
            userName: userName,
            otpCode: otpCode,
            expiryMinutes: expiryMinutes,
            heading: "Reset Your Password",
            subtext: "Use the code below to reset your password. If you didn't request this, ignore this email.",
            accentColor: "#f59e0b"
        );

        await SendEmailAsync(toEmail, subject, body);
    }

    // ── Private ───────────────────────────────────────────────────────────

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var senderName   = _configuration["EmailSettings:SenderName"] ?? "Canteen System";
        var senderEmail  = _configuration["EmailSettings:SenderEmail"]
                           ?? throw new InvalidOperationException("EmailSettings:SenderEmail is not configured.");
        var smtpServer   = _configuration["EmailSettings:SmtpServer"]
                           ?? throw new InvalidOperationException("EmailSettings:SmtpServer is not configured.");
        var smtpPort     = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        var smtpUser     = _configuration["EmailSettings:SmtpUser"]
                           ?? throw new InvalidOperationException("EmailSettings:SmtpUser is not configured.");
        var smtpPassword = _configuration["EmailSettings:SmtpPassword"]
                           ?? throw new InvalidOperationException("EmailSettings:SmtpPassword is not configured.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email} | Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    private static string BuildOtpEmailTemplate(
        string userName, string otpCode, int expiryMinutes,
        string heading, string subtext, string accentColor)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>{heading}</title>
        </head>
        <body style="margin:0;padding:0;background-color:#f3f4f6;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="padding:40px 0;">
            <tr>
              <td align="center">
                <table width="520" cellpadding="0" cellspacing="0"
                       style="background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 1px 8px rgba(0,0,0,0.08);">

                  <!-- Header -->
                  <tr>
                    <td style="background:{accentColor};padding:28px 32px;text-align:center;">
                      <span style="font-size:28px;">🍽️</span>
                      <h1 style="margin:8px 0 0;color:#ffffff;font-size:20px;font-weight:700;">
                        Canteen System
                      </h1>
                    </td>
                  </tr>

                  <!-- Content -->
                  <tr>
                    <td style="padding:40px 32px 32px;">
                      <h2 style="margin:0 0 12px;color:#111827;font-size:22px;font-weight:700;">{heading}</h2>
                      <p style="margin:0 0 8px;color:#374151;font-size:15px;">Hi <strong>{userName}</strong>,</p>
                      <p style="margin:0 0 32px;color:#6b7280;font-size:14px;line-height:1.6;">{subtext}</p>

                      <!-- OTP Box -->
                      <div style="background:#f9fafb;border:2px dashed {accentColor};border-radius:10px;
                                  padding:28px;text-align:center;margin-bottom:28px;">
                        <p style="margin:0 0 10px;color:#9ca3af;font-size:11px;
                                  text-transform:uppercase;letter-spacing:2px;font-weight:600;">
                          One-Time Password
                        </p>
                        <p style="margin:0;color:#111827;font-size:44px;font-weight:800;
                                  letter-spacing:14px;font-family:'Courier New',monospace;">
                          {otpCode}
                        </p>
                      </div>

                      <p style="margin:0;color:#9ca3af;font-size:13px;text-align:center;">
                        ⏱️ Expires in <strong>{expiryMinutes} minutes</strong> &nbsp;|&nbsp;
                        Do not share this code with anyone.
                      </p>
                    </td>
                  </tr>

                  <!-- Footer -->
                  <tr>
                    <td style="padding:20px 32px;border-top:1px solid #e5e7eb;text-align:center;">
                      <p style="margin:0;color:#d1d5db;font-size:12px;">
                        If you didn't request this, you can safely ignore this email.
                      </p>
                    </td>
                  </tr>

                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
    }
}