using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable(
            "leave_balances",
            (table) =>
            {
                table.HasCheckConstraint("ck_leave_balances_annual_quota", "annual_quota >= 0");
                table.HasCheckConstraint("ck_leave_balances_carried_over", "carried_over >= 0");
                table.HasCheckConstraint("ck_leave_balances_annual_used", "annual_used >= 0");
                table.HasCheckConstraint("ck_leave_balances_annual_reserved", "annual_reserved >= 0");
                table.HasCheckConstraint("ck_leave_balances_carry_over_used", "carry_over_used >= 0");
                table.HasCheckConstraint("ck_leave_balances_carry_over_reserved", "carry_over_reserved >= 0");
            }
        );

        builder.Property((leaveBalance) => leaveBalance.Id).HasConversion<LeaveBalanceIdConverter>();
        builder.Property((leaveBalance) => leaveBalance.EmployeeId).HasConversion<EmployeeIdConverter>();
        builder.Property((leaveBalance) => leaveBalance.AnnualQuota).HasConversion<LeaveDaysConverter>();
        builder.Property((leaveBalance) => leaveBalance.CarriedOver).HasConversion<LeaveDaysConverter>();
        builder.Property((leaveBalance) => leaveBalance.AnnualUsed).HasConversion<LeaveDaysConverter>();
        builder.Property((leaveBalance) => leaveBalance.AnnualReserved).HasConversion<LeaveDaysConverter>();
        builder.Property((leaveBalance) => leaveBalance.CarryOverUsed).HasConversion<LeaveDaysConverter>();
        builder.Property((leaveBalance) => leaveBalance.CarryOverReserved).HasConversion<LeaveDaysConverter>();
        builder.Property<uint>("xmin").IsRowVersion();

        builder.HasIndex((leaveBalance) => new { leaveBalance.EmployeeId, leaveBalance.Year })
            .IsUnique()
            .HasDatabaseName("ux_leave_balances_employee_year");

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey((leaveBalance) => leaveBalance.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany((leaveBalance) => leaveBalance.Adjustments)
            .WithOne()
            .HasForeignKey((adjustment) => adjustment.LeaveBalanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation((leaveBalance) => leaveBalance.Adjustments)
            .HasField("_adjustments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}