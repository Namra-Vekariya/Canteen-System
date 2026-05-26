using System.ComponentModel.DataAnnotations;

namespace CanteenSystem.Application.DTOs.User;

public class UpdateProfileRequest
{
    [Required]
    [MaxLength(100)] 
    public string Name { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)] 
    public string? Phone { get; set; }
}