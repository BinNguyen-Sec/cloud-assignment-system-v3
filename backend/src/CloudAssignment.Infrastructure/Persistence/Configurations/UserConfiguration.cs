using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudAssignment.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.StudentCode).HasMaxLength(32);
        builder.Property(user => user.FullName).HasMaxLength(160).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(254).IsRequired();
        builder.Property(user => user.NormalizedEmail).HasMaxLength(254).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.MustChangePassword).IsRequired();
        builder.Property(user => user.CreatedAtUtc).IsRequired();
        builder.Property(user => user.UpdatedAtUtc).IsRequired();

        builder.HasIndex(user => user.NormalizedEmail).IsUnique();
        builder.HasIndex(user => user.StudentCode)
            .IsUnique()
            .HasFilter("\"StudentCode\" IS NOT NULL");
        builder.HasIndex(user => new { user.Role, user.IsActive });
    }
}
