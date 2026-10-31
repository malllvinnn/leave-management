using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence.Converters;
using LeaveManagement.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property((applicationUser) => applicationUser.EmployeeId).HasConversion<EmployeeIdConverter>();

        builder.HasIndex((applicationUser) => applicationUser.EmployeeId)
            .IsUnique()
            .HasDatabaseName("ux_aspnetusers_employee_id");

        builder.HasOne<Employee>()
            .WithOne()
            .HasForeignKey<ApplicationUser>((applicationUser) => applicationUser.EmployeeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}