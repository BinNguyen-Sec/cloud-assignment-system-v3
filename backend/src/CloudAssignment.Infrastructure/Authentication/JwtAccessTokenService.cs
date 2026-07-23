using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CloudAssignment.Application.Abstractions.Authentication;
using CloudAssignment.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CloudAssignment.Infrastructure.Authentication;

public sealed class JwtAccessTokenService(IOptions<JwtOptions> options) : IAccessTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessTokenResult Create(User user, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(user);
        var expiresAtUtc = utcNow.ToUniversalTime().AddMinutes(_options.AccessTokenMinutes);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<System.Security.Claims.Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new("role", user.Role.ToString()),
            new("must_change_password", user.MustChangePassword ? "true" : "false")
        };

        if (!string.IsNullOrWhiteSpace(user.StudentCode))
        {
            claims.Add(new System.Security.Claims.Claim("student_code", user.StudentCode));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: utcNow.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: signingCredentials);

        return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
