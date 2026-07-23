using CloudAssignment.Domain.Courses;

namespace CloudAssignment.UnitTests.Domain;

public sealed class CourseTests
{
    [Fact]
    public void CreateNormalizesCodeAndStartsActive()
    {
        var course = Course.Create(
            Guid.NewGuid(),
            " cloud-101 ",
            "Cloud Security",
            null,
            "Semester 1",
            "2026-2027",
            Guid.NewGuid(),
            "astral",
            DateTimeOffset.UtcNow);

        Assert.Equal("CLOUD-101", course.Code);
        Assert.False(course.IsArchived);
    }

    [Fact]
    public void ArchiveAndRestoreUpdateLifecycleState()
    {
        var createdAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var course = Course.Create(
            Guid.NewGuid(),
            "SEC-101",
            "Security",
            null,
            null,
            null,
            Guid.NewGuid(),
            "runes",
            createdAt);

        course.Archive(createdAt.AddMinutes(1));
        Assert.True(course.IsArchived);
        course.Restore(createdAt.AddMinutes(2));
        Assert.False(course.IsArchived);
    }
}
