using CloudAssignment.Domain.Common;

namespace CloudAssignment.Domain.Authentication;

public sealed class RefreshToken : Entity
{
    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        string? createdByIp)
        : base(id)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            expiresAtUtc,
            createdAtUtc);

        UserId = userId;
        TokenHash = string.IsNullOrWhiteSpace(tokenHash)
            ? throw new ArgumentException("Token hash is required.", nameof(tokenHash))
            : tokenHash;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        ExpiresAtUtc = expiresAtUtc.ToUniversalTime();
        CreatedByIp = TrimIp(createdByIp);
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsActiveAt(DateTimeOffset utcNow) =>
        RevokedAtUtc is null && ExpiresAtUtc > utcNow.ToUniversalTime();

    public static RefreshToken Create(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        string? createdByIp) =>
        new(id, userId, tokenHash, createdAtUtc, expiresAtUtc, createdByIp);

    public void Revoke(DateTimeOffset revokedAtUtc, string? revokedByIp, Guid? replacedByTokenId = null)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RevokedAtUtc = revokedAtUtc.ToUniversalTime();
        RevokedByIp = TrimIp(revokedByIp);
        ReplacedByTokenId = replacedByTokenId;
    }

    private static string? TrimIp(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, 64)];
}

