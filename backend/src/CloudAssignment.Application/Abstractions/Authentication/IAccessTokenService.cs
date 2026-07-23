using CloudAssignment.Domain.Users;

namespace CloudAssignment.Application.Abstractions.Authentication;

public interface IAccessTokenService
{
    AccessTokenResult Create(User user, DateTimeOffset utcNow);
}

public sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAtUtc);
