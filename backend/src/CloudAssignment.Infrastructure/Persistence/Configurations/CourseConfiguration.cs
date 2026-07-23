using CloudAssignment.Domain.Courses;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudAssignment.Infrastructure.Persistence.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(course => course.Id);
        builder.Property(course => course.Code).HasMaxLength(40).IsRequired();
        builder.Property(course => course.Name).HasMaxLength(180).IsRequired();
        builder.Property(course => course.Description).HasMaxLength(4000);
        builder.Property(course => course.Semester).HasMaxLength(30);
        builder.Property(course => course.AcademicYear).HasMaxLength(20);
        builder.Property(course => course.ThemeKey).HasMaxLength(40).IsRequired();
        builder.Property(course => course.CreatedAtUtc).IsRequired();
        builder.Property(course => course.UpdatedAtUtc).IsRequired();
        builder.HasIndex(course => course.Code).IsUnique();
        builder.HasIndex(course => new { course.TeacherId, course.IsArchived });
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(course => course.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
