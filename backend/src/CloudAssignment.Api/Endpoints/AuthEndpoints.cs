using CloudAssignment.Api.Authentication;
using CloudAssignment.Application.Features.Auth;

namespace CloudAssignment.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth").WithTags("Authentication");

        group.MapPost("/login", LoginAsync).AllowAnonymous();
        group.MapPost("/refresh", RefreshAsync).AllowAnonymous();
        group.MapPost("/logout", LogoutAsync).AllowAnonymous();
        group.MapGet("/me", GetCurrentUserAsync).RequireAuthorization();
        group.MapPost("/change-password", ChangePasswordAsync).RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        RefreshTokenCookieManager cookieManager,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.LoginAsync(
            request,
            GetIpAddress(httpContext),
            cancellationToken);
        cookieManager.Write(
            httpContext.Response,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc);
        return Results.Ok(result.Session);
    }

    private static async Task<IResult> RefreshAsync(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        RefreshTokenCookieManager cookieManager,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.RefreshAsync(
            cookieManager.Read(httpContext.Request) ?? string.Empty,
            GetIpAddress(httpContext),
            cancellationToken);
        cookieManager.Write(
            httpContext.Response,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc);
        return Results.Ok(result.Session);
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        RefreshTokenCookieManager cookieManager,
        CancellationToken cancellationToken)
    {
        await authenticationService.LogoutAsync(
            cookieManager.Read(httpContext.Request),
            GetIpAddress(httpContext),
            cancellationToken);
        cookieManager.Delete(httpContext.Response);
        return Results.NoContent();
    }

    private static async Task<IResult> GetCurrentUserAsync(
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken) =>
        Results.Ok(await authenticationService.GetCurrentUserAsync(cancellationToken));

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        HttpContext httpContext,
        IAuthenticationService authenticationService,
        RefreshTokenCookieManager cookieManager,
        CancellationToken cancellationToken)
    {
        await authenticationService.ChangePasswordAsync(
            request,
            GetIpAddress(httpContext),
            cancellationToken);
        cookieManager.Delete(httpContext.Response);
        return Results.NoContent();
    }

    private static string? GetIpAddress(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString();
}
