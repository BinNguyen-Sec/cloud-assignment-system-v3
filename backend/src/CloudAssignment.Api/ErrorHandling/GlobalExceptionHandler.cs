using CloudAssignment.Application.Common.Exceptions;
using CloudAssignment.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CloudAssignment.Api.ErrorHandling;

public sealed partial class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (
            status,
            title,
            errorCode,
            errors,
            logLevel) = exception switch
        {
            RequestValidationException validation => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                validation.ErrorCode,
                validation.Errors,
                LogLevel.Information),

            ForbiddenException forbidden => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                forbidden.ErrorCode,
                null,
                LogLevel.Warning),

            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                notFound.ErrorCode,
                null,
                LogLevel.Information),

            ConflictException conflict => (
                StatusCodes.Status409Conflict,
                "Conflict",
                conflict.ErrorCode,
                null,
                LogLevel.Information),

            DomainException domain => (
                StatusCodes.Status422UnprocessableEntity,
                "Business rule violation",
                domain.Code,
                null,
                LogLevel.Information),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Unexpected server error",
                "SYSTEM_UNEXPECTED_ERROR",
                null,
                LogLevel.Error)
        };

        LogRequestFailure(
            logger,
            logLevel,
            errorCode,
            httpContext.TraceIdentifier,
            exception);

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,

            Detail =
                status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred. Use the traceId when contacting support."
                    : exception.Message,

            Instance = httpContext.Request.Path,

            Type =
                $"https://cloud-assignment/errors/" +
                $"{errorCode.ToLowerInvariant().Replace('_', '-')}"
        };

        problem.Extensions["errorCode"] = errorCode;
        problem.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
    }

    [LoggerMessage(
        EventId = 1000,
        Message =
            "Request failed with {ErrorCode}. TraceId: {TraceId}")]
    private static partial void LogRequestFailure(
        ILogger logger,
        LogLevel logLevel,
        string errorCode,
        string traceId,
        Exception exception);
}
