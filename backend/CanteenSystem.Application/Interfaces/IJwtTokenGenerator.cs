using CanteenSystem.Domain.Entities;

namespace CanteenSystem.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    (string Token, DateTime ExpiresAt) GenerateRefreshToken();
}