using System.Security.Cryptography;
using System.Text;
using CloudAssignment.Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;

namespace CloudAssignment.Infrastructure.Authentication;

public sealed class RefreshTokenService(IOptions<JwtOptions> options) : IRefreshTokenService
{
    public TimeSpan Lifetime { get; } = TimeSpan.FromDays(options.Value.RefreshTokenDays);
    public GeneratedRefreshToken Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var plainTextToken = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        return new GeneratedRefreshToken(plainTextToken, Hash(plainTextToken));
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
