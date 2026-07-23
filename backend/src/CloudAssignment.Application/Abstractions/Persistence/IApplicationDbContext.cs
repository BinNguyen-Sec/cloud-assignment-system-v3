using CloudAssignment.Domain.Auditing;
using CloudAssignment.Domain.Authentication;
using CloudAssignment.Domain.Courses;
using CloudAssignment.Domain.StudentImports;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CloudAssignment.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Course> Courses { get; }

    DbSet<CourseMember> CourseMembers { get; }

    DbSet<StudentImportBatch> StudentImportBatches { get; }

    DbSet<StudentImportRow> StudentImportRows { get; }

    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
