using CloudAssignment.Application.Abstractions.Authentication;
using CloudAssignment.Application.Abstractions.Persistence;
using CloudAssignment.Application.Common.Exceptions;
using CloudAssignment.Domain.Courses;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CloudAssignment.Application.Features.Courses;

internal sealed class CourseAccessService(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser)
{
    public async Task<User> RequireCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException("AUTH_REQUIRED", "Vui lòng đăng nhập để tiếp tục.");
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == currentUser.UserId.Value, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("AUTH_SESSION_INVALID", "Phiên đăng nhập không còn hợp lệ.");
        }

        return user;
    }

    public async Task<(Course Course, User User)> RequireOwnedCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var user = await RequireCurrentUserAsync(cancellationToken);
        if (user.Role != UserRole.Teacher)
        {
            throw new ForbiddenException("COURSE_TEACHER_REQUIRED", "Chỉ giảng viên mới có thể quản lý môn học.");
        }

        var course = await dbContext.Courses
            .SingleOrDefaultAsync(candidate => candidate.Id == courseId, cancellationToken)
            ?? throw CourseNotFound();

        if (course.TeacherId != user.Id)
        {
            throw CourseNotFound();
        }

        return (course, user);
    }

    public async Task<(Course Course, User User, bool CanManage)> RequireCourseAccessAsync(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var user = await RequireCurrentUserAsync(cancellationToken);
        var course = await dbContext.Courses
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == courseId, cancellationToken)
            ?? throw CourseNotFound();

        var canManage = user.Role == UserRole.Admin ||
            (user.Role == UserRole.Teacher && course.TeacherId == user.Id);

        var canView = canManage ||
            (user.Role == UserRole.Student &&
             await dbContext.CourseMembers.AsNoTracking().AnyAsync(
                 member => member.CourseId == courseId && member.StudentId == user.Id,
                 cancellationToken));

        if (!canView)
        {
            throw CourseNotFound();
        }

        return (course, user, canManage);
    }

    public static NotFoundException CourseNotFound() =>
        new("COURSE_NOT_FOUND", "Môn học không tồn tại hoặc nằm ngoài phạm vi truy cập của bạn.");
}
