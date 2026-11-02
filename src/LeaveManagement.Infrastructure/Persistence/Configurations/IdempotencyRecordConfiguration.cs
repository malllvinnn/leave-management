using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence.Converters;
using LeaveManagement.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records");

        builder.Property((idempotencyRecord) => idempotencyRecord.EmployeeId).HasConversion<EmployeeIdConverter>();
        builder.Property((idempotencyRecord) => idempotencyRecord.Operation).HasMaxLength(100);
        builder.Property((idempotencyRecord) => idempotencyRecord.IdempotencyKey).HasMaxLength(100);
        builder.Property((idempotencyRecord) => idempotencyRecord.RequestHash).HasMaxLength(64);
        builder.Property((idempotencyRecord) => idempotencyRecord.ResultEntityType).HasMaxLength(100);
        builder.Property((idempotencyRecord) => idempotencyRecord.ResultEntityId).HasMaxLength(100);

        builder.HasIndex((idempotencyRecord) => new { idempotencyRecord.EmployeeId, idempotencyRecord.Operation, idempotencyRecord.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("ux_idempotency_records_employee_operation_key")
            .HasFilter("completed_at IS NULL");

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey((idempotencyRecord) => idempotencyRecord.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}