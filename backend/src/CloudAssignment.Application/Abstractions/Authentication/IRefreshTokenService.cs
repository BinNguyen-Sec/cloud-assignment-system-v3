namespace CloudAssignment.Application.Abstractions.Authentication;

public interface IRefreshTokenService
{
    TimeSpan Lifetime { get; }

    GeneratedRefreshToken Generate();

    string Hash(string token);
}

public sealed record GeneratedRefreshToken(string PlainTextToken, string TokenHash);
