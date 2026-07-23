using CloudAssignment.Api.Authentication;
using CloudAssignment.Application.Features.Courses;
using CloudAssignment.Application.Features.Enrollments;
using CloudAssignment.Application.Features.StudentImports;
using Microsoft.AspNetCore.Mvc;

namespace CloudAssignment.Api.Endpoints;

public static class CourseEndpoints
{
    public static IEndpointRouteBuilder MapCourseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var courses = endpoints.MapGroup("/api/v1/courses")
            .WithTags("Courses")
            .RequireAuthorization();

        courses.MapGet("/", GetCoursesAsync);
        courses.MapPost("/", CreateCourseAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly);
        courses.MapGet("/{courseId:guid}", GetCourseAsync);
        courses.MapPut("/{courseId:guid}", UpdateCourseAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly);
        courses.MapDelete("/{courseId:guid}", DeleteCourseAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly);
        courses.MapPost("/{courseId:guid}/archive", ArchiveCourseAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly);
        courses.MapPost("/{courseId:guid}/restore", RestoreCourseAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly);

        courses.MapGet("/{courseId:guid}/students", GetStudentsAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);
        courses.MapPost("/{courseId:guid}/students", EnrollStudentAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly);
        courses.MapDelete("/{courseId:guid}/students/{studentId:guid}", RemoveStudentAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly);

        courses.MapGet("/{courseId:guid}/students/import-template", GetImportTemplate)
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly);
        courses.MapPost("/{courseId:guid}/students/import-preview", PreviewImportAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly)
            .DisableAntiforgery();
        courses.MapPost("/{courseId:guid}/students/imports/{batchId:guid}/confirm", ConfirmImportAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly);
        courses.MapGet("/{courseId:guid}/students/imports", GetCourseImportHistoryAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);
        courses.MapGet("/{courseId:guid}/students/imports/{batchId:guid}", GetImportBatchAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);
        courses.MapGet("/{courseId:guid}/students/imports/{batchId:guid}/error-report", GetImportReportAsync)
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);

        endpoints.MapGet("/api/v1/student-imports", GetImportHistoryAsync)
            .WithTags("Student Imports")
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);

        return endpoints;
    }

    private static async Task<IResult> GetCoursesAsync(
        [FromQuery(Name = "q")] string? query,
        string? sort,
        string? direction,
        int? page,
        int? pageSize,
        string? semester,
        string? academicYear,
        bool? archived,
        ICourseService service,
        CancellationToken cancellationToken)
    {
        var response = await service.GetCoursesAsync(
            new CourseListRequest(
                query,
                sort,
                direction,
                NormalizePage(page),
                NormalizePageSize(pageSize),
                semester,
                academicYear,
                archived),
            cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetCourseAsync(
        Guid courseId,
        ICourseService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.GetCourseAsync(courseId, cancellationToken));

    private static async Task<IResult> CreateCourseAsync(
        CreateCourseRequest request,
        HttpContext httpContext,
        ICourseService service,
        CancellationToken cancellationToken)
    {
        var course = await service.CreateCourseAsync(
            request,
            CreateAuditContext(httpContext),
            cancellationToken);
        return Results.Created($"/api/v1/courses/{course.Id}", course);
    }

    private static async Task<IResult> UpdateCourseAsync(
        Guid courseId,
        UpdateCourseRequest request,
        HttpContext httpContext,
        ICourseService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.UpdateCourseAsync(
            courseId,
            request,
            CreateAuditContext(httpContext),
            cancellationToken));

    private static async Task<IResult> ArchiveCourseAsync(
        Guid courseId,
        HttpContext httpContext,
        ICourseService service,
        CancellationToken cancellationToken)
    {
        await service.ArchiveCourseAsync(
            courseId,
            CreateAuditContext(httpContext),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RestoreCourseAsync(
        Guid courseId,
        HttpContext httpContext,
        ICourseService service,
        CancellationToken cancellationToken)
    {
        await service.RestoreCourseAsync(
            courseId,
            CreateAuditContext(httpContext),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteCourseAsync(
        Guid courseId,
        HttpContext httpContext,
        ICourseService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteCourseAsync(
            courseId,
            CreateAuditContext(httpContext),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetStudentsAsync(
        Guid courseId,
        [FromQuery(Name = "q")] string? query,
        string? sort,
        string? direction,
        int? page,
        int? pageSize,
        IEnrollmentService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.GetStudentsAsync(
            courseId,
            new CourseStudentListRequest(
                query,
                sort,
                direction,
                NormalizePage(page),
                NormalizePageSize(pageSize)),
            cancellationToken));

    private static async Task<IResult> EnrollStudentAsync(
        Guid courseId,
        EnrollStudentRequest request,
        HttpContext httpContext,
        IEnrollmentService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.EnrollStudentAsync(
            courseId,
            request,
            CreateAuditContext(httpContext),
            cancellationToken));

    private static async Task<IResult> RemoveStudentAsync(
        Guid courseId,
        Guid studentId,
        HttpContext httpContext,
        IEnrollmentService service,
        CancellationToken cancellationToken)
    {
        await service.RemoveStudentAsync(
            courseId,
            studentId,
            CreateAuditContext(httpContext),
            cancellationToken);
        return Results.NoContent();
    }

    private static IResult GetImportTemplate(IStudentImportService service)
    {
        var file = service.CreateTemplate();
        return Results.File(file.Content, file.ContentType, file.FileName);
    }

    private static async Task<IResult> PreviewImportAsync(
        Guid courseId,
        [FromForm] IFormFile file,
        HttpContext httpContext,
        IStudentImportService service,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var preview = await service.PreviewAsync(
            courseId,
            file.FileName,
            file.Length,
            stream,
            CreateAuditContext(httpContext),
            cancellationToken);
        return Results.Ok(preview);
    }

    private static async Task<IResult> ConfirmImportAsync(
        Guid courseId,
        Guid batchId,
        HttpContext httpContext,
        IStudentImportService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.ConfirmAsync(
            courseId,
            batchId,
            CreateAuditContext(httpContext),
            cancellationToken));

    private static async Task<IResult> GetCourseImportHistoryAsync(
        Guid courseId,
        int? page,
        int? pageSize,
        string? status,
        IStudentImportService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.GetHistoryAsync(
            new StudentImportHistoryRequest(
                NormalizePage(page),
                NormalizePageSize(pageSize),
                status,
                courseId),
            cancellationToken));

    private static async Task<IResult> GetImportHistoryAsync(
        int? page,
        int? pageSize,
        string? status,
        Guid? courseId,
        IStudentImportService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.GetHistoryAsync(
            new StudentImportHistoryRequest(
                NormalizePage(page),
                NormalizePageSize(pageSize),
                status,
                courseId),
            cancellationToken));

    private static async Task<IResult> GetImportBatchAsync(
        Guid courseId,
        Guid batchId,
        IStudentImportService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.GetBatchAsync(courseId, batchId, cancellationToken));

    private static async Task<IResult> GetImportReportAsync(
        Guid courseId,
        Guid batchId,
        IStudentImportService service,
        CancellationToken cancellationToken)
    {
        var file = await service.CreateErrorReportAsync(courseId, batchId, cancellationToken);
        return Results.File(file.Content, file.ContentType, file.FileName);
    }

    private static AuditRequestContext CreateAuditContext(HttpContext httpContext) =>
        new(
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers["User-Agent"].ToString());

    private static int NormalizePage(int? value) => value is null or 0 ? 1 : value.Value;

    private static int NormalizePageSize(int? value) => value is null or 0 ? 20 : value.Value;
}
