#pragma warning disable CA1862 // EF Core query translation does not support StringComparison overloads.
using CloudAssignment.Application.Abstractions.Persistence;
using CloudAssignment.Application.Abstractions.Time;
using CloudAssignment.Application.Common.Exceptions;
using CloudAssignment.Application.Common.Models;
using CloudAssignment.Domain.Courses;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CloudAssignment.Application.Features.Courses;

internal sealed class CourseService(
    IApplicationDbContext dbContext,
    CourseAccessService accessService,
    AuditLogFactory auditLogFactory,
    IClock clock) : ICourseService
{
    private static readonly string[] AllowedThemeKeys =
    [
        "astral",
        "alchemy",
        "runes",
        "celestial",
        "botany",
        "crystal"
    ];

    public async Task<PagedResponse<CourseSummaryDto>> GetCoursesAsync(
        CourseListRequest request,
        CancellationToken cancellationToken)
    {
        var user = await accessService.RequireCurrentUserAsync(cancellationToken);
        var page = CreatePageRequest(request.Page, request.PageSize);
        var query = dbContext.Courses.AsNoTracking();

        query = user.Role switch
        {
            UserRole.Admin => query,
            UserRole.Teacher => query.Where(course => course.TeacherId == user.Id),
            UserRole.Student => query.Where(course =>
                dbContext.CourseMembers.Any(member =>
                    member.CourseId == course.Id && member.StudentId == user.Id)),
            _ => throw new ForbiddenException(
                "COURSE_ROLE_UNSUPPORTED",
                "Vai trò hiện tại không thể xem môn học.")
        };

        if (request.Archived is not null)
        {
            query = query.Where(course => course.IsArchived == request.Archived.Value);
        }

#pragma warning disable CA1304, CA1311 // EF Core translates parameterless ToUpper() to SQL UPPER().
        if (!string.IsNullOrWhiteSpace(request.Semester))
        {
            var semester = request.Semester.Trim().ToUpperInvariant();
            query = query.Where(course =>
                course.Semester != null &&
                course.Semester.ToUpper() == semester);
        }

        if (!string.IsNullOrWhiteSpace(request.AcademicYear))
        {
            var academicYear = request.AcademicYear.Trim().ToUpperInvariant();
            query = query.Where(course =>
                course.AcademicYear != null &&
                course.AcademicYear.ToUpper() == academicYear);
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var search = request.Query.Trim().ToUpperInvariant();
            query = query.Where(course =>
                course.Code.ToUpper().Contains(search) ||
                course.Name.ToUpper().Contains(search) ||
                (course.Description != null &&
                    course.Description.ToUpper().Contains(search)) ||
                (course.Semester != null &&
                    course.Semester.ToUpper().Contains(search)) ||
                (course.AcademicYear != null &&
                    course.AcademicYear.ToUpper().Contains(search)) ||
                dbContext.Users.Any(teacher =>
                    teacher.Id == course.TeacherId &&
                    teacher.FullName.ToUpper().Contains(search)));
        }
#pragma warning restore CA1304, CA1311

        var totalItems = await query.LongCountAsync(cancellationToken);

        query = ApplySort(
            query,
            dbContext,
            request.Sort,
            request.Direction);

        var rows = await query
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(course => new CourseQueryRow(
                course.Id,
                course.Code,
                course.Name,
                course.Description,
                course.Semester,
                course.AcademicYear,
                course.TeacherId,
                dbContext.Users
                    .Where(teacher => teacher.Id == course.TeacherId)
                    .Select(teacher => teacher.FullName)
                    .Single(),
                course.IsArchived,
                course.ThemeKey,
                dbContext.CourseMembers.Count(member =>
                    member.CourseId == course.Id),
                course.CreatedAtUtc,
                course.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResponse<CourseSummaryDto>(
            rows.Select(MapSummary).ToList(),
            page.Page,
            page.PageSize,
            totalItems);
    }

    public async Task<CourseDetailDto> GetCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var (_, _, canManage) = await accessService.RequireCourseAccessAsync(courseId, cancellationToken);
        return await MapDetailAsync(courseId, canManage, cancellationToken);
    }

    public async Task<CourseDetailDto> CreateCourseAsync(
        CreateCourseRequest request,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken)
    {
        var user = await accessService.RequireCurrentUserAsync(cancellationToken);
        if (user.Role != UserRole.Teacher)
        {
            throw new ForbiddenException("COURSE_TEACHER_REQUIRED", "Chỉ giảng viên mới có thể tạo môn học.");
        }

        Validate(request.Code, request.Name, request.Description, request.Semester, request.AcademicYear, request.ThemeKey);
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var codeExists = await dbContext.Courses
            .AnyAsync(course => course.Code == normalizedCode, cancellationToken);
        if (codeExists)
        {
            throw new ConflictException("COURSE_CODE_EXISTS", "Mã môn học đã tồn tại.");
        }

        var course = Course.Create(
            Guid.NewGuid(),
            normalizedCode,
            request.Name,
            request.Description,
            request.Semester,
            request.AcademicYear,
            user.Id,
            request.ThemeKey,
            clock.UtcNow);

        dbContext.Courses.Add(course);
        dbContext.AuditLogs.Add(auditLogFactory.Create(
            "COURSE_CREATED",
            nameof(Course),
            course.Id,
            new { course.Code, course.Name },
            auditContext));
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapDetailAsync(course.Id, canManage: true, cancellationToken);
    }

    public async Task<CourseDetailDto> UpdateCourseAsync(
        Guid courseId,
        UpdateCourseRequest request,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken)
    {
        var (course, _) = await accessService.RequireOwnedCourseAsync(courseId, cancellationToken);
        Validate(request.Code, request.Name, request.Description, request.Semester, request.AcademicYear, request.ThemeKey);
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var codeExists = await dbContext.Courses.AnyAsync(
            candidate => candidate.Id != courseId && candidate.Code == normalizedCode,
            cancellationToken);
        if (codeExists)
        {
            throw new ConflictException("COURSE_CODE_EXISTS", "Mã môn học đã tồn tại.");
        }

        course.Update(
            normalizedCode,
            request.Name,
            request.Description,
            request.Semester,
            request.AcademicYear,
            request.ThemeKey,
            clock.UtcNow);

        dbContext.AuditLogs.Add(auditLogFactory.Create(
            "COURSE_UPDATED",
            nameof(Course),
            course.Id,
            new { course.Code, course.Name },
            auditContext));
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapDetailAsync(course.Id, canManage: true, cancellationToken);
    }

    public async Task ArchiveCourseAsync(
        Guid courseId,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken)
    {
        var (course, _) = await accessService.RequireOwnedCourseAsync(courseId, cancellationToken);
        course.Archive(clock.UtcNow);
        dbContext.AuditLogs.Add(auditLogFactory.Create(
            "COURSE_ARCHIVED",
            nameof(Course),
            course.Id,
            new { course.Code },
            auditContext));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreCourseAsync(
        Guid courseId,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken)
    {
        var (course, _) = await accessService.RequireOwnedCourseAsync(courseId, cancellationToken);
        course.Restore(clock.UtcNow);
        dbContext.AuditLogs.Add(auditLogFactory.Create(
            "COURSE_RESTORED",
            nameof(Course),
            course.Id,
            new { course.Code },
            auditContext));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCourseAsync(
        Guid courseId,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken)
    {
        var (course, _) = await accessService.RequireOwnedCourseAsync(courseId, cancellationToken);
        var hasMembers = await dbContext.CourseMembers
            .AnyAsync(member => member.CourseId == courseId, cancellationToken);
        if (hasMembers)
        {
            throw new ConflictException(
                "COURSE_DELETE_HAS_MEMBERS",
                "Không thể xóa môn học đang có sinh viên. Hãy lưu trữ môn học thay vì xóa.");
        }

        dbContext.Courses.Remove(course);
        dbContext.AuditLogs.Add(auditLogFactory.Create(
            "COURSE_DELETED",
            nameof(Course),
            course.Id,
            new { course.Code, course.Name },
            auditContext));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<CourseDetailDto> MapDetailAsync(
        Guid courseId,
        bool canManage,
        CancellationToken cancellationToken)
    {
        var detail = await dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Id == courseId)
            .Select(course => new CourseDetailDto(
                course.Id,
                course.Code,
                course.Name,
                course.Description,
                course.Semester,
                course.AcademicYear,
                course.TeacherId,
                dbContext.Users
                    .Where(teacher => teacher.Id == course.TeacherId)
                    .Select(teacher => teacher.FullName)
                    .Single(),
                course.IsArchived,
                course.ThemeKey,
                dbContext.CourseMembers.Count(member => member.CourseId == course.Id),
                0,
                course.CreatedAtUtc,
                course.UpdatedAtUtc,
                canManage))
            .SingleOrDefaultAsync(cancellationToken);

        return detail ?? throw CourseAccessService.CourseNotFound();
    }

    private static IQueryable<Course> ApplySort(
        IQueryable<Course> query,
        IApplicationDbContext dbContext,
        string? sort,
        string? direction)
    {
        var descending =
            !string.Equals(
                direction,
                "asc",
                StringComparison.OrdinalIgnoreCase);

        /*
         * Name is the provider-neutral default. PostgreSQL still supports
         * explicit createdAt/updatedAt sorting; SQLite integration tests do
         * not reliably support ORDER BY on DateTimeOffset.
         */
        var sortKey =
            string.IsNullOrWhiteSpace(sort)
                ? "name"
                : sort.Trim();

        return sortKey switch
        {
            "name" => descending
                ? query
                    .OrderByDescending(course => course.Name)
                    .ThenBy(course => course.Code)
                : query
                    .OrderBy(course => course.Name)
                    .ThenBy(course => course.Code),

            "code" => descending
                ? query.OrderByDescending(course => course.Code)
                : query.OrderBy(course => course.Code),

            "createdAt" => descending
                ? query.OrderByDescending(course => course.CreatedAtUtc)
                : query.OrderBy(course => course.CreatedAtUtc),

            "studentCount" => descending
                ? query
                    .OrderByDescending(course =>
                        dbContext.CourseMembers.Count(member =>
                            member.CourseId == course.Id))
                    .ThenBy(course => course.Name)
                : query
                    .OrderBy(course =>
                        dbContext.CourseMembers.Count(member =>
                            member.CourseId == course.Id))
                    .ThenBy(course => course.Name),

            "updatedAt" => descending
                ? query.OrderByDescending(course => course.UpdatedAtUtc)
                : query.OrderBy(course => course.UpdatedAtUtc),

            _ => throw new RequestValidationException(
                new Dictionary<string, string[]>
                {
                    ["sort"] = ["Giá trị sắp xếp không hợp lệ."]
                })
        };
    }

    private static CourseSummaryDto MapSummary(CourseQueryRow row) =>
        new(
            row.Id,
            row.Code,
            row.Name,
            row.Description,
            row.Semester,
            row.AcademicYear,
            row.TeacherId,
            row.TeacherName,
            row.IsArchived,
            row.ThemeKey,
            row.StudentCount,
            0,
            row.CreatedAtUtc,
            row.UpdatedAtUtc);

    private static PageRequest CreatePageRequest(int page, int pageSize)
    {
        try
        {
            return new PageRequest(page, pageSize);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new RequestValidationException(
                new Dictionary<string, string[]>
                {
                    ["pagination"] = ["Page phải từ 1 và pageSize phải nằm trong khoảng 1–50."]
                });
        }
    }

    private static void Validate(
        string code,
        string name,
        string? description,
        string? semester,
        string? academicYear,
        string themeKey)
    {
        var errors = new Dictionary<string, string[]>();
        AddRequiredLengthError(errors, "code", code, 40, "Mã môn học");
        AddRequiredLengthError(errors, "name", name, 180, "Tên môn học");
        AddOptionalLengthError(errors, "description", description, 4000, "Mô tả");
        AddOptionalLengthError(errors, "semester", semester, 30, "Học kỳ");
        AddOptionalLengthError(errors, "academicYear", academicYear, 20, "Năm học");

        if (!AllowedThemeKeys.Contains(themeKey, StringComparer.OrdinalIgnoreCase))
        {
            errors["themeKey"] = ["Chủ đề môn học không hợp lệ."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }
    }

    private static void AddRequiredLengthError(
        Dictionary<string, string[]> errors,
        string key,
        string? value,
        int maximumLength,
        string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[key] = [$"{displayName} là bắt buộc."];
        }
        else if (value.Trim().Length > maximumLength)
        {
            errors[key] = [$"{displayName} không được vượt quá {maximumLength} ký tự."];
        }
    }

    private static void AddOptionalLengthError(
        Dictionary<string, string[]> errors,
        string key,
        string? value,
        int maximumLength,
        string displayName)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maximumLength)
        {
            errors[key] = [$"{displayName} không được vượt quá {maximumLength} ký tự."];
        }
    }

    private sealed record CourseQueryRow(
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
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}

#pragma warning restore CA1862
