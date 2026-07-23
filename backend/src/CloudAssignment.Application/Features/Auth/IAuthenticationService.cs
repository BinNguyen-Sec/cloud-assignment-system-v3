namespace CloudAssignment.Application.Features.Auth;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<AuthenticationResult> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task LogoutAsync(
        string? refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken);

    Task<UserSessionDto> GetCurrentUserAsync(CancellationToken cancellationToken);

    Task ChangePasswordAsync(
        ChangePasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken);
}
