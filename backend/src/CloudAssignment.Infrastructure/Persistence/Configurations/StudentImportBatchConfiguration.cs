using CloudAssignment.Domain.Courses;
using CloudAssignment.Domain.StudentImports;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudAssignment.Infrastructure.Persistence.Configurations;

public sealed class StudentImportBatchConfiguration : IEntityTypeConfiguration<StudentImportBatch>
{
    public void Configure(EntityTypeBuilder<StudentImportBatch> builder)
    {
        builder.ToTable("StudentImportBatches");
        builder.HasKey(batch => batch.Id);
        builder.Property(batch => batch.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(batch => batch.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(batch => batch.CreatedAtUtc).IsRequired();
        builder.Property(batch => batch.ExpiresAtUtc).IsRequired();
        builder.HasIndex(batch => new { batch.CourseId, batch.CreatedAtUtc });
        builder.HasIndex(batch => new { batch.UploadedById, batch.CreatedAtUtc });
        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(batch => batch.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(batch => batch.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
