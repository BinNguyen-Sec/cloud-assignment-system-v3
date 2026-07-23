using CloudAssignment.Application.Abstractions.Time;

namespace CloudAssignment.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
