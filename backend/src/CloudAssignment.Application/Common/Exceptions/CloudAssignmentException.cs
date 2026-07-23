namespace CloudAssignment.Application.Common.Exceptions;

public abstract class CloudAssignmentException : Exception
{
    protected CloudAssignmentException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
