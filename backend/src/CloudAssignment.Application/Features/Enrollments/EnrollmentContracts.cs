using CloudAssignment.Application.Common.Models;
using CloudAssignment.Application.Features.Courses;

namespace CloudAssignment.Application.Features.Enrollments;

public sealed record CourseStudentListRequest(
    string? Query,
    string? Sort,
    string? Direction,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize);

public sealed record EnrollStudentRequest(string Email);

public sealed record CourseStudentDto(
    Guid UserId,
    string? StudentCode,
    string FullName,
    string Email,
    string EnrollmentSource,
    DateTimeOffset EnrolledAtUtc);

public interface IEnrollmentService
{
    Task<PagedResponse<CourseStudentDto>> GetStudentsAsync(
        Guid courseId,
        CourseStudentListRequest request,
        CancellationToken cancellationToken);

    Task<CourseStudentDto> EnrollStudentAsync(
        Guid courseId,
        EnrollStudentRequest request,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken);

    Task RemoveStudentAsync(
        Guid courseId,
        Guid studentId,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken);
}
