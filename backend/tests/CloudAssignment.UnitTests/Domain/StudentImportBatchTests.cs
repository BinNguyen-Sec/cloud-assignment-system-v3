using CloudAssignment.Domain.StudentImports;

namespace CloudAssignment.UnitTests.Domain;

public sealed class StudentImportBatchTests
{
    [Fact]
    public void CompleteStoresFinalCounts()
    {
        var now = DateTimeOffset.UtcNow;
        var batch = StudentImportBatch.CreatePreview(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "students.xlsx",
            5,
            3,
            2,
            now,
            now.AddMinutes(30));

        batch.Complete(3, 2, now.AddMinutes(1));

        Assert.Equal(StudentImportBatchStatus.Completed, batch.Status);
        Assert.Equal(3, batch.ImportedRows);
        Assert.Equal(2, batch.SkippedRows);
    }

    [Fact]
    public void PreviewExpiresAtConfiguredTime()
    {
        var now = DateTimeOffset.UtcNow;
        var batch = StudentImportBatch.CreatePreview(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "students.xlsx",
            1,
            1,
            0,
            now,
            now.AddMinutes(30));

        Assert.False(batch.IsExpiredAt(now.AddMinutes(29)));
        Assert.True(batch.IsExpiredAt(now.AddMinutes(30)));
    }
}
