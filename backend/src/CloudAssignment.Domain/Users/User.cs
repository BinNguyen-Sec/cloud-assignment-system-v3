using CloudAssignment.Domain.Common;

namespace CloudAssignment.Domain.Users;

public sealed class User : AuditableEntity
{
    private User()
    {
    }

    private User(
        Guid id,
        string? studentCode,
        string fullName,
        string email,
        string normalizedEmail,
        string passwordHash,
        UserRole role,
        DateTimeOffset createdAtUtc)
        : base(id, createdAtUtc)
    {
        StudentCode = NormalizeOptional(studentCode);
        FullName = Require(fullName, nameof(fullName), 160);
        Email = Require(email, nameof(email), 254);
        NormalizedEmail = Require(normalizedEmail, nameof(normalizedEmail), 254);
        PasswordHash = Require(passwordHash, nameof(passwordHash), 500);
        Role = role;
        IsActive = true;
    }

    public string? StudentCode { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string NormalizedEmail { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public bool MustChangePassword { get; private set; }

    public static User Create(
        Guid id,
        string? studentCode,
        string fullName,
        string email,
        string normalizedEmail,
        string passwordHash,
        UserRole role,
        DateTimeOffset createdAtUtc,
        bool mustChangePassword = false)
    {
        var user = new User(
            id,
            studentCode,
            fullName,
            email,
            normalizedEmail,
            passwordHash,
            role,
            createdAtUtc);
        user.MustChangePassword = mustChangePassword;
        return user;
    }

    public void ChangePassword(string passwordHash, DateTimeOffset changedAtUtc)
    {
        PasswordHash = Require(passwordHash, nameof(passwordHash), 500);
        MustChangePassword = false;
        MarkUpdated(changedAtUtc);
    }

    public void SetActive(bool isActive, DateTimeOffset changedAtUtc)
    {
        IsActive = isActive;
        MarkUpdated(changedAtUtc);
    }

    private static string Require(string value, string parameterName, int maximumLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Maximum length is {maximumLength}.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
