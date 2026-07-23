namespace CloudAssignment.Application.Features.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);

public sealed record UserSessionDto(
    Guid Id,
    string? StudentCode,
    string FullName,
    string Email,
    string Role,
    bool MustChangePassword);

public sealed record AuthSessionDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    UserSessionDto User);

public sealed record AuthenticationResult(
    AuthSessionDto Session,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);
