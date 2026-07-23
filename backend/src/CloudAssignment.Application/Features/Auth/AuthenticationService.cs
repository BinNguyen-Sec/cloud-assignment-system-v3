using CloudAssignment.Application.Abstractions.Authentication;
using CloudAssignment.Application.Abstractions.Persistence;
using CloudAssignment.Application.Abstractions.Time;
using CloudAssignment.Application.Common.Exceptions;
using CloudAssignment.Domain.Authentication;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CloudAssignment.Application.Features.Auth;

public sealed class AuthenticationService(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    ICurrentUser currentUser,
    IClock clock) : IAuthenticationService
{
    public async Task<AuthenticationResult> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        ValidateLogin(request);
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException(
                "AUTH_INVALID_CREDENTIALS",
                "Email hoặc mật khẩu không chính xác.");
        }

        return await CreateSessionAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthenticationResult> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw InvalidRefreshToken();
        }

        var utcNow = clock.UtcNow;
        var tokenHash = refreshTokenService.Hash(refreshToken);
        var existingToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActiveAt(utcNow))
        {
            throw InvalidRefreshToken();
        }

        var user = await dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == existingToken.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw InvalidRefreshToken();
        }

        var generated = refreshTokenService.Generate();
        var replacement = RefreshToken.Create(
            Guid.NewGuid(),
            user.Id,
            generated.TokenHash,
            utcNow,
            utcNow.Add(refreshTokenService.Lifetime),
            ipAddress);

        existingToken.Revoke(utcNow, ipAddress, replacement.Id);
        dbContext.RefreshTokens.Add(replacement);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BuildResult(user, generated.PlainTextToken, replacement.ExpiresAtUtc, utcNow);
    }

    public async Task LogoutAsync(
        string? refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = refreshTokenService.Hash(refreshToken);
        var existingToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || existingToken.RevokedAtUtc is not null)
        {
            return;
        }

        existingToken.Revoke(clock.UtcNow, ipAddress);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserSessionDto> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = RequireCurrentUserId();
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("AUTH_SESSION_INVALID", "Phiên đăng nhập không còn hợp lệ.");
        }

        return MapUser(user);
    }

    public async Task ChangePasswordAsync(
        ChangePasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var userId = RequireCurrentUserId();
        ValidateChangePassword(request);
        var user = await dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
            ?? throw new UnauthorizedException("AUTH_SESSION_INVALID", "Phiên đăng nhập không còn hợp lệ.");

        if (!user.IsActive)
        {
            throw new UnauthorizedException("AUTH_ACCOUNT_DISABLED", "Tài khoản đã bị vô hiệu hóa.");
        }

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("AUTH_CURRENT_PASSWORD_INVALID", "Mật khẩu hiện tại không chính xác.");
        }

        if (passwordHasher.Verify(request.NewPassword, user.PasswordHash))
        {
            throw new RequestValidationException(
                new Dictionary<string, string[]>
                {
                    ["newPassword"] = ["Mật khẩu mới phải khác mật khẩu hiện tại."]
                });
        }

        var utcNow = clock.UtcNow;
        user.ChangePassword(passwordHasher.Hash(request.NewPassword), utcNow);

        var activeTokens = await dbContext.RefreshTokens
            .Where(token => token.UserId == user.Id && token.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(utcNow, ipAddress);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthenticationResult> CreateSessionAsync(
        User user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var utcNow = clock.UtcNow;
        var generated = refreshTokenService.Generate();
        var refreshToken = RefreshToken.Create(
            Guid.NewGuid(),
            user.Id,
            generated.TokenHash,
            utcNow,
            utcNow.Add(refreshTokenService.Lifetime),
            ipAddress);

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return BuildResult(user, generated.PlainTextToken, refreshToken.ExpiresAtUtc, utcNow);
    }

    private AuthenticationResult BuildResult(
        User user,
        string refreshToken,
        DateTimeOffset refreshTokenExpiresAtUtc,
        DateTimeOffset utcNow)
    {
        var accessToken = accessTokenService.Create(user, utcNow);
        return new AuthenticationResult(
            new AuthSessionDto(accessToken.Token, accessToken.ExpiresAtUtc, MapUser(user)),
            refreshToken,
            refreshTokenExpiresAtUtc);
    }

    private Guid RequireCurrentUserId()
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException("AUTH_REQUIRED", "Vui lòng đăng nhập để tiếp tục.");
        }

        return currentUser.UserId.Value;
    }

    private static UserSessionDto MapUser(User user) =>
        new(user.Id, user.StudentCode, user.FullName, user.Email, user.Role.ToString(), user.MustChangePassword);

    private static void ValidateLogin(LoginRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email là bắt buộc."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = ["Mật khẩu là bắt buộc."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }
    }

    private static void ValidateChangePassword(ChangePasswordRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            errors["currentPassword"] = ["Mật khẩu hiện tại là bắt buộc."];
        }

        if (!string.Equals(request.NewPassword, request.ConfirmNewPassword, StringComparison.Ordinal))
        {
            errors["confirmNewPassword"] = ["Xác nhận mật khẩu không khớp."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        PasswordPolicy.Validate(request.NewPassword);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private static UnauthorizedException InvalidRefreshToken() =>
        new("AUTH_REFRESH_TOKEN_INVALID", "Phiên đăng nhập đã hết hạn hoặc không hợp lệ.");
}
