using CloudAssignment.Domain.Common;

namespace CloudAssignment.Domain.Courses;

public sealed class Course : AuditableEntity
{
    private Course()
    {
    }

    private Course(
        Guid id,
        string code,
        string name,
        string? description,
        string? semester,
        string? academicYear,
        Guid teacherId,
        string themeKey,
        DateTimeOffset createdAtUtc)
        : base(id, createdAtUtc)
    {
        Code = NormalizeCode(code);
        Name = Require(name, nameof(name), 180);
        Description = NormalizeOptional(description, 4000);
        Semester = NormalizeOptional(semester, 30);
        AcademicYear = NormalizeOptional(academicYear, 20);
        TeacherId = RequireId(teacherId, nameof(teacherId));
        ThemeKey = Require(themeKey, nameof(themeKey), 40);
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Semester { get; private set; }

    public string? AcademicYear { get; private set; }

    public Guid TeacherId { get; private set; }

    public bool IsArchived { get; private set; }

    public string ThemeKey { get; private set; } = string.Empty;

    public static Course Create(
        Guid id,
        string code,
        string name,
        string? description,
        string? semester,
        string? academicYear,
        Guid teacherId,
        string themeKey,
        DateTimeOffset createdAtUtc) =>
        new(
            id,
            code,
            name,
            description,
            semester,
            academicYear,
            teacherId,
            themeKey,
            createdAtUtc);

    public void Update(
        string code,
        string name,
        string? description,
        string? semester,
        string? academicYear,
        string themeKey,
        DateTimeOffset updatedAtUtc)
    {
        Code = NormalizeCode(code);
        Name = Require(name, nameof(name), 180);
        Description = NormalizeOptional(description, 4000);
        Semester = NormalizeOptional(semester, 30);
        AcademicYear = NormalizeOptional(academicYear, 20);
        ThemeKey = Require(themeKey, nameof(themeKey), 40);
        MarkUpdated(updatedAtUtc);
    }

    public void Archive(DateTimeOffset updatedAtUtc)
    {
        if (IsArchived)
        {
            return;
        }

        IsArchived = true;
        MarkUpdated(updatedAtUtc);
    }

    public void Restore(DateTimeOffset updatedAtUtc)
    {
        if (!IsArchived)
        {
            return;
        }

        IsArchived = false;
        MarkUpdated(updatedAtUtc);
    }

    private static string NormalizeCode(string value) =>
        Require(value, nameof(value), 40).ToUpperInvariant();

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

    private static Guid RequireId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier cannot be empty.", parameterName);
        }

        return value;
    }
}
