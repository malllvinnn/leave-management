using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class ApprovalDecisionConfiguration : IEntityTypeConfiguration<ApprovalDecision>
{
    public void Configure(EntityTypeBuilder<ApprovalDecision> builder)
    {
        builder.ToTable("approval_decisions");

        builder.Property((approvalDecision) => approvalDecision.LeaveRequestId).HasConversion<LeaveRequestIdConverter>();
        builder.Property((approvalDecision) => approvalDecision.ActorId).HasConversion<EmployeeIdConverter>();
        builder.Property((approvalDecision) => approvalDecision.Reason).HasConversion<ReasonConverter>().HasMaxLength(500);
        builder.Property((approvalDecision) => approvalDecision.BypassedApproverId).HasConversion<EmployeeIdConverter>();

        builder.HasOne<LeaveRequest>()
            .WithMany()
            .HasForeignKey((approvalDecision) => approvalDecision.LeaveRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey((approvalDecision) => approvalDecision.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey((approvalDecision) => approvalDecision.BypassedApproverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}