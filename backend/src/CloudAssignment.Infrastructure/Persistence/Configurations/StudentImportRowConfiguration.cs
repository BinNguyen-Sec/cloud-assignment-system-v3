using CloudAssignment.Domain.StudentImports;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudAssignment.Infrastructure.Persistence.Configurations;

public sealed class StudentImportRowConfiguration : IEntityTypeConfiguration<StudentImportRow>
{
    public void Configure(EntityTypeBuilder<StudentImportRow> builder)
    {
        builder.ToTable("StudentImportRows");
        builder.HasKey(row => row.Id);
        builder.Property(row => row.StudentCode).HasMaxLength(32);
        builder.Property(row => row.FullName).HasMaxLength(160);
        builder.Property(row => row.Email).HasMaxLength(254);
        builder.Property(row => row.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(row => row.Message).HasMaxLength(1000);
        builder.HasIndex(row => new { row.BatchId, row.RowNumber }).IsUnique();
        builder.HasOne<StudentImportBatch>()
            .WithMany()
            .HasForeignKey(row => row.BatchId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(row => row.ResolvedUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
