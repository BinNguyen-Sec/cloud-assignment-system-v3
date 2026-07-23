namespace CloudAssignment.Application.Common.Exceptions;

public sealed class RequestValidationException : CloudAssignmentException
{
    public RequestValidationException(
        IReadOnlyDictionary<string, string[]> errors,
        string message = "One or more validation errors occurred.")
        : base("VALIDATION_FAILED", message)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
