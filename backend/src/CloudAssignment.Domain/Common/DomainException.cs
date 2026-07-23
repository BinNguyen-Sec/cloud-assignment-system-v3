namespace CloudAssignment.Domain.Common;

public sealed class DomainException : Exception
{
    public DomainException(string code, string message)
        : base(message)
    {
        Code = string.IsNullOrWhiteSpace(code)
            ? throw new ArgumentException("Error code is required.", nameof(code))
            : code;
    }

    public string Code { get; }
}
