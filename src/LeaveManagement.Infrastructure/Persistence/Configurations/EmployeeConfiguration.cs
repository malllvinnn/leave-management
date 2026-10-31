using LeaveManagement.Domain.Entities;
using LeaveManagement.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagement.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        builder.Property((employee) => employee.Id).HasConversion<EmployeeIdConverter>();
        builder.Property((employee) => employee.FullName).HasMaxLength(200);
        builder.Property((employee) => employee.Email).HasConversion<EmailConverter>().HasMaxLength(255);
        builder.Property((employee) => employee.ManagerId).HasConversion<EmployeeIdConverter>();
        builder.Property((employee) => employee.PositionId).HasConversion<PositionIdConverter>();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey((employee) => employee.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Position>()
            .WithMany()
            .HasForeignKey((employee) => employee.PositionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}