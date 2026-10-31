using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class JobExecutionMarkerConfiguration : IEntityTypeConfiguration<JobExecutionMarker>
{
    public void Configure(EntityTypeBuilder<JobExecutionMarker> builder)
    {
        builder.ToTable("job_execution_markers");

        builder.Property((jobExecutionMarker) => jobExecutionMarker.JobName).HasMaxLength(100);
        builder.Property((jobExecutionMarker) => jobExecutionMarker.PeriodKey).HasMaxLength(50);

        builder.HasIndex((jobExecutionMarker) => new { jobExecutionMarker.JobName, jobExecutionMarker.PeriodKey })
            .IsUnique()
            .HasDatabaseName("ux_job_execution_markers_job_name_period_key");
    }
}