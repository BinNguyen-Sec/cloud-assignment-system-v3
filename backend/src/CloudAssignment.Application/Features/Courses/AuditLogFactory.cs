using System.Text.Json;
using CloudAssignment.Application.Abstractions.Authentication;
using CloudAssignment.Application.Abstractions.Time;
using CloudAssignment.Domain.Auditing;

namespace CloudAssignment.Application.Features.Courses;

internal sealed class AuditLogFactory(
    ICurrentUser currentUser,
    IClock clock)
{
    public AuditLog Create(
        string action,
        string entityType,
        Guid? entityId,
        object metadata,
        AuditRequestContext context) =>
        AuditLog.Create(
            Guid.NewGuid(),
            currentUser.UserId,
            action,
            entityType,
            entityId,
            JsonSerializer.Serialize(metadata),
            context.IpAddress,
            context.UserAgent,
            clock.UtcNow);
}
