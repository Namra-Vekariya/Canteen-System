using System.ComponentModel.DataAnnotations;

namespace CanteenSystem.Application.DTOs.Auth;

public class VerifyEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string OtpCode { get; set; } = string.Empty;
}