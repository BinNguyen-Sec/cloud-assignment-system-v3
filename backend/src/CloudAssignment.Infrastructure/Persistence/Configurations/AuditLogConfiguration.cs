using CloudAssignment.Domain.Auditing;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudAssignment.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.Action).HasMaxLength(100).IsRequired();
        builder.Property(log => log.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(log => log.MetadataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(log => log.IpAddress).HasMaxLength(64);
        builder.Property(log => log.UserAgent).HasMaxLength(512);
        builder.Property(log => log.CreatedAtUtc).IsRequired();
        builder.HasIndex(log => log.CreatedAtUtc);
        builder.HasIndex(log => new { log.ActorUserId, log.CreatedAtUtc });
        builder.HasIndex(log => new { log.EntityType, log.EntityId, log.CreatedAtUtc });
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(log => log.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
