using CloudAssignment.Application.Abstractions.Persistence;
using CloudAssignment.Domain.Auditing;
using CloudAssignment.Domain.Authentication;
using CloudAssignment.Domain.Courses;
using CloudAssignment.Domain.StudentImports;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CloudAssignment.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<CourseMember> CourseMembers => Set<CourseMember>();

    public DbSet<StudentImportBatch> StudentImportBatches => Set<StudentImportBatch>();

    public DbSet<StudentImportRow> StudentImportRows => Set<StudentImportRow>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
