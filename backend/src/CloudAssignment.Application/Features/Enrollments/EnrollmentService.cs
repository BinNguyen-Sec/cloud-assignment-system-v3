#pragma warning disable CA1862 // EF Core query translation does not support StringComparison overloads.
using System.ComponentModel.DataAnnotations;
using CloudAssignment.Application.Abstractions.Persistence;
using CloudAssignment.Application.Abstractions.Time;
using CloudAssignment.Application.Common.Exceptions;
using CloudAssignment.Application.Common.Models;
using CloudAssignment.Application.Features.Courses;
using CloudAssignment.Domain.Courses;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CloudAssignment.Application.Features.Enrollments;

internal sealed class EnrollmentService(
    IApplicationDbContext dbContext,
    CourseAccessService accessService,
    AuditLogFactory auditLogFactory,
    IClock clock) : IEnrollmentService
{
    public async Task<PagedResponse<CourseStudentDto>> GetStudentsAsync(
        Guid courseId,
        CourseStudentListRequest request,
        CancellationToken cancellationToken)
    {
        var (_, user, canManage) =
            await accessService.RequireCourseAccessAsync(
                courseId,
                cancellationToken);

        if (!canManage || user.Role == UserRole.Student)
        {
            throw new ForbiddenException(
                "COURSE_STUDENTS_FORBIDDEN",
                "Bạn không có quyền xem danh sách sinh viên của môn học này.");
        }

        var page = CreatePageRequest(request.Page, request.PageSize);

        var query = dbContext.CourseMembers
            .AsNoTracking()
            .Where(member => member.CourseId == courseId);

#pragma warning disable CA1304, CA1311 // EF Core translates parameterless ToUpper() to SQL UPPER().
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var search = request.Query.Trim().ToUpperInvariant();

            query = query.Where(member =>
                dbContext.Users.Any(student =>
                    student.Id == member.StudentId &&
                    (
                        student.FullName.ToUpper().Contains(search) ||
                        student.Email.ToUpper().Contains(search) ||
                        (
                            student.StudentCode != null &&
                            student.StudentCode.ToUpper().Contains(search)
                        )
                    )));
        }
#pragma warning restore CA1304, CA1311

        var totalItems = await query.LongCountAsync(cancellationToken);

        query = ApplySort(
            query,
            dbContext,
            request.Sort,
            request.Direction);

        var students = await query
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(member => new CourseStudentQueryRow(
                member.StudentId,
                dbContext.Users
                    .Where(student => student.Id == member.StudentId)
                    .Select(student => student.StudentCode)
                    .Single(),
                dbContext.Users
                    .Where(student => student.Id == member.StudentId)
                    .Select(student => student.FullName)
                    .Single(),
                dbContext.Users
                    .Where(student => student.Id == member.StudentId)
                    .Select(student => student.Email)
                    .Single(),
                member.EnrollmentSource,
                member.EnrolledAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResponse<CourseStudentDto>(
            students.Select(MapStudent).ToList(),
            page.Page,
            page.PageSize,
            totalItems);
    }

    public async Task<CourseStudentDto> EnrollStudentAsync(
        Guid courseId,
        EnrollStudentRequest request,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken)
    {
        var (course, _) = await accessService.RequireOwnedCourseAsync(courseId, cancellationToken);
        ValidateEmail(request.Email);
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var student = await dbContext.Users.SingleOrDefaultAsync(
            user => user.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (student is null)
        {
            throw new NotFoundException(
                "ENROLLMENT_STUDENT_NOT_FOUND",
                "Không tìm thấy tài khoản sinh viên với email đã nhập.");
        }

        if (student.Role != UserRole.Student)
        {
            throw new ConflictException(
                "ENROLLMENT_WRONG_ROLE",
                "Tài khoản được chọn không có vai trò Student.");
        }

        if (!student.IsActive)
        {
            throw new ConflictException(
                "ENROLLMENT_INACTIVE_USER",
                "Tài khoản sinh viên đang bị vô hiệu hóa.");
        }

        var exists = await dbContext.CourseMembers.AnyAsync(
            member => member.CourseId == courseId && member.StudentId == student.Id,
            cancellationToken);
        if (exists)
        {
            throw new ConflictException(
                "ENROLLMENT_ALREADY_EXISTS",
                "Sinh viên đã có trong môn học.");
        }

        var enrolledAtUtc = clock.UtcNow;
        var member = CourseMember.Create(
            Guid.NewGuid(),
            courseId,
            student.Id,
            EnrollmentSource.Manual,
            importBatchId: null,
            enrolledAtUtc);
        dbContext.CourseMembers.Add(member);
        dbContext.AuditLogs.Add(auditLogFactory.Create(
            "COURSE_STUDENT_ENROLLED",
            nameof(CourseMember),
            member.Id,
            new { course.Id, course.Code, StudentId = student.Id, student.Email, Source = "Manual" },
            auditContext));
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CourseStudentDto(
            student.Id,
            student.StudentCode,
            student.FullName,
            student.Email,
            member.EnrollmentSource.ToString(),
            enrolledAtUtc);
    }

    public async Task RemoveStudentAsync(
        Guid courseId,
        Guid studentId,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken)
    {
        var (course, _) = await accessService.RequireOwnedCourseAsync(courseId, cancellationToken);
        var member = await dbContext.CourseMembers.SingleOrDefaultAsync(
            candidate => candidate.CourseId == courseId && candidate.StudentId == studentId,
            cancellationToken);
        if (member is null)
        {
            throw new NotFoundException(
                "ENROLLMENT_NOT_FOUND",
                "Sinh viên không có trong môn học.");
        }

        var student = await dbContext.Users
            .AsNoTracking()
            .SingleAsync(user => user.Id == studentId, cancellationToken);
        dbContext.CourseMembers.Remove(member);
        dbContext.AuditLogs.Add(auditLogFactory.Create(
            "COURSE_STUDENT_REMOVED",
            nameof(CourseMember),
            member.Id,
            new { course.Id, course.Code, StudentId = student.Id, student.Email },
            auditContext));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<CourseMember> ApplySort(
        IQueryable<CourseMember> query,
        IApplicationDbContext dbContext,
        string? sort,
        string? direction)
    {
        var descending =
            string.Equals(
                direction,
                "desc",
                StringComparison.OrdinalIgnoreCase);

        var sortKey =
            string.IsNullOrWhiteSpace(sort)
                ? "fullName"
                : sort.Trim();

        return sortKey switch
        {
            "fullName" => descending
                ? query.OrderByDescending(member =>
                    dbContext.Users
                        .Where(student => student.Id == member.StudentId)
                        .Select(student => student.FullName)
                        .Single())
                : query.OrderBy(member =>
                    dbContext.Users
                        .Where(student => student.Id == member.StudentId)
                        .Select(student => student.FullName)
                        .Single()),

            "studentCode" => descending
                ? query.OrderByDescending(member =>
                    dbContext.Users
                        .Where(student => student.Id == member.StudentId)
                        .Select(student => student.StudentCode)
                        .Single())
                : query.OrderBy(member =>
                    dbContext.Users
                        .Where(student => student.Id == member.StudentId)
                        .Select(student => student.StudentCode)
                        .Single()),

            "email" => descending
                ? query.OrderByDescending(member =>
                    dbContext.Users
                        .Where(student => student.Id == member.StudentId)
                        .Select(student => student.Email)
                        .Single())
                : query.OrderBy(member =>
                    dbContext.Users
                        .Where(student => student.Id == member.StudentId)
                        .Select(student => student.Email)
                        .Single()),

            "enrolledAt" => descending
                ? query.OrderByDescending(member => member.EnrolledAtUtc)
                : query.OrderBy(member => member.EnrolledAtUtc),

            _ => throw new RequestValidationException(
                new Dictionary<string, string[]>
                {
                    ["sort"] = ["Giá trị sắp xếp sinh viên không hợp lệ."]
                })
        };
    }

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

    private static CourseStudentDto MapStudent(CourseStudentQueryRow student) =>
        new(
            student.UserId,
            student.StudentCode,
            student.FullName,
            student.Email,
            student.EnrollmentSource.ToString(),
            student.EnrolledAtUtc);

    private sealed record CourseStudentQueryRow(
        Guid UserId,
        string? StudentCode,
        string FullName,
        string Email,
        EnrollmentSource EnrollmentSource,
        DateTimeOffset EnrolledAtUtc);

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email.Trim()))
        {
            throw new RequestValidationException(
                new Dictionary<string, string[]>
                {
                    ["email"] = ["Email sinh viên không hợp lệ."]
                });
        }
    }
}

#pragma warning restore CA1862
