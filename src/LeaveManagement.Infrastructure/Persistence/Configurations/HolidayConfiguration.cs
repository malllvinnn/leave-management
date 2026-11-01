using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("holidays");

        builder.Property((holiday) => holiday.Name).HasMaxLength(150);

        builder.HasIndex((holiday) => holiday.Date)
            .IsUnique()
            .HasDatabaseName("ux_holidays_date");
    }
}