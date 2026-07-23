using Microsoft.Extensions.Options;

namespace CloudAssignment.Api.Authentication;

public sealed class RefreshTokenCookieManager(IOptions<RefreshCookieOptions> options)
{
    private readonly RefreshCookieOptions _options = options.Value;

    public string? Read(HttpRequest request) =>
        request.Cookies.TryGetValue(_options.Name, out var value) ? value : null;

    public void Write(HttpResponse response, string token, DateTimeOffset expiresAtUtc)
    {
        response.Cookies.Append(_options.Name, token, CreateOptions(expiresAtUtc));
    }

    public void Delete(HttpResponse response)
    {
        response.Cookies.Delete(_options.Name, CreateOptions(DateTimeOffset.UnixEpoch));
    }

    private CookieOptions CreateOptions(DateTimeOffset expiresAtUtc) => new()
    {
        HttpOnly = true,
        Secure = _options.Secure,
        SameSite = ParseSameSite(_options.SameSite),
        Path = "/api/v1/auth",
        Expires = expiresAtUtc,
        IsEssential = true
    };

    private static SameSiteMode ParseSameSite(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "NONE" => SameSiteMode.None,
            "STRICT" => SameSiteMode.Strict,
            _ => SameSiteMode.Lax
        };
}
