using CloudAssignment.Domain.Common;

namespace CloudAssignment.Domain.Auditing;

public sealed class AuditLog : Entity
{
    private AuditLog()
    {
    }

    private AuditLog(
        Guid id,
        Guid? actorUserId,
        string action,
        string entityType,
        Guid? entityId,
        string metadataJson,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        ActorUserId = actorUserId;
        Action = Require(action, nameof(action), 100);
        EntityType = Require(entityType, nameof(entityType), 100);
        EntityId = entityId;
        MetadataJson = Require(metadataJson, nameof(metadataJson), 16000);
        IpAddress = NormalizeOptional(ipAddress, 64);
        UserAgent = NormalizeOptional(userAgent, 512);
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public Guid? ActorUserId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public Guid? EntityId { get; private set; }

    public string MetadataJson { get; private set; } = "{}";

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static AuditLog Create(
        Guid id,
        Guid? actorUserId,
        string action,
        string entityType,
        Guid? entityId,
        string metadataJson,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset createdAtUtc) =>
        new(
            id,
            actorUserId,
            action,
            entityType,
            entityId,
            metadataJson,
            ipAddress,
            userAgent,
            createdAtUtc);

    private static string Require(string value, string parameterName, int maximumLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(normalized.Length, maximumLength, parameterName);
        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(normalized.Length, maximumLength, nameof(value));
        }

        return normalized;
    }
}
