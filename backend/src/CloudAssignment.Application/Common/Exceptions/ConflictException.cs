namespace CloudAssignment.Application.Common.Exceptions;

public sealed class ConflictException : CloudAssignmentException
{
    public ConflictException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
