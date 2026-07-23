namespace CloudAssignment.Application.Common.Exceptions;

public sealed class ForbiddenException : CloudAssignmentException
{
    public ForbiddenException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
