namespace CloudAssignment.Application.Common.Exceptions;

public sealed class NotFoundException : CloudAssignmentException
{
    public NotFoundException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
