using CloudAssignment.Application.Common.Models;

namespace CloudAssignment.Application.Features.Courses;

public sealed record CourseListRequest(
    string? Query,
    string? Sort,
    string? Direction,
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize,
    string? Semester = null,
    string? AcademicYear = null,
    bool? Archived = null);

public sealed record CreateCourseRequest(
    string Code,
    string Name,
    string? Description,
    string? Semester,
    string? AcademicYear,
    string ThemeKey);

public sealed record UpdateCourseRequest(
    string Code,
    string Name,
    string? Description,
    string? Semester,
    string? AcademicYear,
    string ThemeKey);

public sealed record CourseSummaryDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string? Semester,
    string? AcademicYear,
    Guid TeacherId,
    string TeacherName,
    bool IsArchived,
    string ThemeKey,
    int StudentCount,
    int AssignmentCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record CourseDetailDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string? Semester,
    string? AcademicYear,
    Guid TeacherId,
    string TeacherName,
    bool IsArchived,
    string ThemeKey,
    int StudentCount,
    int AssignmentCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    bool CanManage);

public interface ICourseService
{
    Task<PagedResponse<CourseSummaryDto>> GetCoursesAsync(
        CourseListRequest request,
        CancellationToken cancellationToken);

    Task<CourseDetailDto> GetCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken);

    Task<CourseDetailDto> CreateCourseAsync(
        CreateCourseRequest request,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken);

    Task<CourseDetailDto> UpdateCourseAsync(
        Guid courseId,
        UpdateCourseRequest request,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken);

    Task ArchiveCourseAsync(
        Guid courseId,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken);

    Task RestoreCourseAsync(
        Guid courseId,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken);

    Task DeleteCourseAsync(
        Guid courseId,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken);
}

public sealed record AuditRequestContext(
    string? IpAddress,
    string? UserAgent);
