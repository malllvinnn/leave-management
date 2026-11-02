using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace LeaveManagement.Infrastructure.Persistence.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public EmployeeId EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}