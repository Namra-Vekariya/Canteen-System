using System.Security.Cryptography;
using System.Text;

namespace CanteenSystem.Application.Common.Helpers;

public static class CryptoHelper
{
    /// <summary>
    /// SHA-256 → Base64 hash. Output is always 44 characters.
    /// Used for hashing refresh tokens and OTP codes before DB storage.
    /// </summary>
    public static string HashWithSha256(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(sha256.ComputeHash(bytes));
    }
}