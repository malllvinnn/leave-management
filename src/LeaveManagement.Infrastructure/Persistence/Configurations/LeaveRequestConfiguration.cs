using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("leave_requests");

        builder.Property((leaveRequest) => leaveRequest.Id).HasConversion<LeaveRequestIdConverter>();
        builder.Property((leaveRequest) => leaveRequest.EmployeeId).HasConversion<EmployeeIdConverter>();

        builder.OwnsOne(
            (leaveRequest) => leaveRequest.Period,
            (period) =>
            {
                period.Property((p) => p.Start).HasColumnName("start_date");
                period.Property((p) => p.End).HasColumnName("end_date");
            }
        );

        builder.Property((leaveRequest) => leaveRequest.WorkingDays).HasConversion<LeaveDaysConverter>();

        builder.ComplexProperty(
            (leaveRequest) => leaveRequest.Allocation,
            (allocation) =>
            {
                allocation.Property((a) => a.Annual).HasColumnName("annual_allocated").HasConversion<LeaveDaysConverter>();
                allocation.Property((a) => a.CarryOver).HasColumnName("carry_over_allocated").HasConversion<LeaveDaysConverter>();
                allocation.Ignore((a) => a.Total);
            }
        );

        builder.Property((leaveRequest) => leaveRequest.Reason).HasConversion<ReasonConverter>().HasMaxLength(500);
        builder.Property<uint>("xmin").IsRowVersion();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey((leaveRequest) => leaveRequest.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany((leaveRequest) => leaveRequest.Decisions)
            .WithOne()
            .HasForeignKey((decision) => decision.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation((leaveRequest) => leaveRequest.Decisions)
            .HasField("_decisions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}