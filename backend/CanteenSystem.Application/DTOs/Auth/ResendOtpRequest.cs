using System.ComponentModel.DataAnnotations;

namespace CanteenSystem.Application.DTOs.Auth;

public class ResendOtpRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}