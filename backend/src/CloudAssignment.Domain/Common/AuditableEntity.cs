namespace CloudAssignment.Domain.Common;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity()
    {
    }

    protected AuditableEntity(Guid id, DateTimeOffset createdAtUtc)
        : base(id)
    {
        CreatedAtUtc = EnsureUtc(createdAtUtc);
        UpdatedAtUtc = CreatedAtUtc;
    }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    protected void MarkUpdated(DateTimeOffset updatedAtUtc)
    {
        var normalized = EnsureUtc(updatedAtUtc);
        if (normalized < CreatedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(updatedAtUtc),
                "Updated time cannot be earlier than created time.");
        }

        UpdatedAtUtc = normalized;
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset value) => value.ToUniversalTime();
}
