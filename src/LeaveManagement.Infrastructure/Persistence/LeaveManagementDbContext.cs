using LeaveManagement.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Persistence;

public class LeaveManagementDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public LeaveManagementDbContext(DbContextOptions<LeaveManagementDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeaveManagementDbContext).Assembly);
    }

    public static DbContextOptions<LeaveManagementDbContext> CreateOptions(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LeaveManagementDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return optionsBuilder;
    }
}