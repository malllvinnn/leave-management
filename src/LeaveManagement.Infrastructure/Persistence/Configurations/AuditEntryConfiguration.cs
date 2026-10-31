using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries");

        builder.Property((auditEntry) => auditEntry.EntityType).HasMaxLength(100);
        builder.Property((auditEntry) => auditEntry.EntityId).HasMaxLength(100);
        builder.Property((auditEntry) => auditEntry.Action).HasMaxLength(100);
        builder.Property((auditEntry) => auditEntry.ActorId).HasConversion<EmployeeIdConverter>();
        builder.Property((auditEntry) => auditEntry.ActorSnapshot).HasMaxLength(300);
        builder.Property((auditEntry) => auditEntry.Payload).HasColumnType("jsonb");
    }
}