using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class QuotaAdjustmentConfiguration : IEntityTypeConfiguration<QuotaAdjustment>
{
    public void Configure(EntityTypeBuilder<QuotaAdjustment> builder)
    {
        builder.ToTable("quota_adjustments");

        builder.Property((quotaAdjustment) => quotaAdjustment.LeaveBalanceId).HasConversion<LeaveBalanceIdConverter>();
        builder.Property((quotaAdjustment) => quotaAdjustment.AdjustedBy).HasConversion<EmployeeIdConverter>().HasColumnName("adjusted_by_id");
        builder.Property((quotaAdjustment) => quotaAdjustment.Reason).HasConversion<ReasonConverter>().HasMaxLength(500);

        builder.HasOne<LeaveBalance>()
            .WithMany()
            .HasForeignKey((quotaAdjustment) => quotaAdjustment.LeaveBalanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey((quotaAdjustment) => quotaAdjustment.AdjustedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}