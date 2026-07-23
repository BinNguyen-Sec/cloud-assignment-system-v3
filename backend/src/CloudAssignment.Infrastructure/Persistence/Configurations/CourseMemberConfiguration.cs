using CloudAssignment.Domain.Courses;
using CloudAssignment.Domain.StudentImports;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudAssignment.Infrastructure.Persistence.Configurations;

public sealed class CourseMemberConfiguration : IEntityTypeConfiguration<CourseMember>
{
    public void Configure(EntityTypeBuilder<CourseMember> builder)
    {
        builder.ToTable("CourseMembers");
        builder.HasKey(member => member.Id);
        builder.Property(member => member.EnrollmentSource)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(member => member.EnrolledAtUtc).IsRequired();
        builder.HasIndex(member => new { member.CourseId, member.StudentId }).IsUnique();
        builder.HasIndex(member => new { member.StudentId, member.EnrolledAtUtc });
        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(member => member.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(member => member.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StudentImportBatch>()
            .WithMany()
            .HasForeignKey(member => member.ImportBatchId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
