using CloudAssignment.Domain.Common;

namespace CloudAssignment.Domain.StudentImports;

public sealed class StudentImportRow : Entity
{
    private StudentImportRow()
    {
    }

    private StudentImportRow(
        Guid id,
        Guid batchId,
        int rowNumber,
        string? studentCode,
        string? fullName,
        string? email,
        Guid? resolvedUserId,
        StudentImportRowStatus status,
        string? message)
        : base(id)
    {
        BatchId = RequireId(batchId, nameof(batchId));
        ArgumentOutOfRangeException.ThrowIfLessThan(rowNumber, 2);
        RowNumber = rowNumber;
        StudentCode = NormalizeOptional(studentCode, 32);
        FullName = NormalizeOptional(fullName, 160);
        Email = NormalizeOptional(email, 254);
        ResolvedUserId = resolvedUserId;
        Status = status;
        Message = NormalizeOptional(message, 1000);
    }

    public Guid BatchId { get; private set; }

    public int RowNumber { get; private set; }

    public string? StudentCode { get; private set; }

    public string? FullName { get; private set; }

    public string? Email { get; private set; }

    public Guid? ResolvedUserId { get; private set; }

    public StudentImportRowStatus Status { get; private set; }

    public string? Message { get; private set; }

    public static StudentImportRow Create(
        Guid id,
        Guid batchId,
        int rowNumber,
        string? studentCode,
        string? fullName,
        string? email,
        Guid? resolvedUserId,
        StudentImportRowStatus status,
        string? message) =>
        new(
            id,
            batchId,
            rowNumber,
            studentCode,
            fullName,
            email,
            resolvedUserId,
            status,
            message);

    public void MarkImported()
    {
        Status = StudentImportRowStatus.Imported;
        Message = "Đã thêm vào môn học.";
    }

    public void MarkSkipped(StudentImportRowStatus status, string message)
    {
        Status = status;
        Message = NormalizeOptional(message, 1000);
    }

    private static Guid RequireId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier cannot be empty.", parameterName);
        }

        return value;
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
