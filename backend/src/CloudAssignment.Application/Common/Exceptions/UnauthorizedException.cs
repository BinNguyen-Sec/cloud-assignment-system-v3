namespace CloudAssignment.Application.Common.Exceptions;

public sealed class UnauthorizedException : CloudAssignmentException
{
    public UnauthorizedException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
