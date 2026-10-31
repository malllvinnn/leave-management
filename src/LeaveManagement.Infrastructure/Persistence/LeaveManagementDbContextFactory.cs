using Microsoft.EntityFrameworkCore.Design;

namespace LeaveManagement.Infrastructure.Persistence;

public sealed class LeaveManagementDbContextFactory : IDesignTimeDbContextFactory<LeaveManagementDbContext>
{
    public LeaveManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__LeaveManagement")
            ?? "Host=localhost;Database=leave_management;Username=postgres";

        var leaveManagementDbContext = new LeaveManagementDbContext(LeaveManagementDbContext.CreateOptions(connectionString));

        return leaveManagementDbContext;
    }
}