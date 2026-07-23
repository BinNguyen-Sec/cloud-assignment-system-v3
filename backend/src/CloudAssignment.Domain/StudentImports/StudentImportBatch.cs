using CloudAssignment.Domain.Common;

namespace CloudAssignment.Domain.StudentImports;

public sealed class StudentImportBatch : Entity
{
    private StudentImportBatch()
    {
    }

    private StudentImportBatch(
        Guid id,
        Guid courseId,
        Guid uploadedById,
        string originalFileName,
        int totalRows,
        int validRows,
        int invalidRows,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
        : base(id)
    {
        CourseId = RequireId(courseId, nameof(courseId));
        UploadedById = RequireId(uploadedById, nameof(uploadedById));
        OriginalFileName = Require(originalFileName, nameof(originalFileName), 255);
        ArgumentOutOfRangeException.ThrowIfNegative(totalRows);
        ArgumentOutOfRangeException.ThrowIfNegative(validRows);
        ArgumentOutOfRangeException.ThrowIfNegative(invalidRows);
        TotalRows = totalRows;
        ValidRows = validRows;
        InvalidRows = invalidRows;
        Status = StudentImportBatchStatus.Previewed;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        ExpiresAtUtc = expiresAtUtc.ToUniversalTime();
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(ExpiresAtUtc, CreatedAtUtc);
    }

    public Guid CourseId { get; private set; }

    public Guid UploadedById { get; private set; }

    public string OriginalFileName { get; private set; } = string.Empty;

    public StudentImportBatchStatus Status { get; private set; }

    public int TotalRows { get; private set; }

    public int ValidRows { get; private set; }

    public int InvalidRows { get; private set; }

    public int ImportedRows { get; private set; }

    public int SkippedRows { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public static StudentImportBatch CreatePreview(
        Guid id,
        Guid courseId,
        Guid uploadedById,
        string originalFileName,
        int totalRows,
        int validRows,
        int invalidRows,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc) =>
        new(
            id,
            courseId,
            uploadedById,
            originalFileName,
            totalRows,
            validRows,
            invalidRows,
            createdAtUtc,
            expiresAtUtc);

    public bool IsExpiredAt(DateTimeOffset utcNow) =>
        utcNow.ToUniversalTime() >= ExpiresAtUtc;

    public void MarkExpired()
    {
        if (Status == StudentImportBatchStatus.Previewed)
        {
            Status = StudentImportBatchStatus.Expired;
        }
    }

    public void Complete(int importedRows, int skippedRows, DateTimeOffset completedAtUtc)
    {
        if (Status != StudentImportBatchStatus.Previewed)
        {
            throw new InvalidOperationException("Only previewed batches can be completed.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(importedRows);
        ArgumentOutOfRangeException.ThrowIfNegative(skippedRows);
        ImportedRows = importedRows;
        SkippedRows = skippedRows;
        Status = StudentImportBatchStatus.Completed;
        CompletedAtUtc = completedAtUtc.ToUniversalTime();
    }

    private static Guid RequireId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier cannot be empty.", parameterName);
        }

        return value;
    }

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
}
