using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Entities;

public class Employee
{
    public EmployeeId Id { get; private set; }
    public string FullName { get; private set; }
    public Email Email { get; private set; }
    public DateOnly HireDate { get; private set; }
    public EmployeeId? ManagerId { get; private set; }
    public PositionId? PositionId { get; private set; }
    public DateTimeOffset? AdminGrantedAt { get; private set; }
    public SystemRole Role { get; private set; }
    public bool IsActive { get; private set; }

    private Employee(
        EmployeeId id,
        string fullName,
        Email email,
        DateOnly hireDate,
        EmployeeId? managerId,
        PositionId? positionId
    )
    {
        Id = id;
        FullName = fullName;
        Email = email;
        HireDate = hireDate;
        ManagerId = managerId;
        PositionId = positionId;
        AdminGrantedAt = null;
        Role = SystemRole.Employee;
        IsActive = true;
    }

    public static Result<Employee> Create(
        string? fullName,
        Email? email,
        DateOnly hireDate,
        EmployeeId? managerId,
        PositionId? positionId
    )
    {
        throw new NotImplementedException();
    }

    public Result AssignManager(EmployeeId? managerId)
    {
        throw new NotImplementedException();
    }

    public Result AssignPosition(PositionId? positionId)
    {
        throw new NotImplementedException();
    }

    public Result GrantAdmin(EmployeeId actorId, DateTimeOffset grantedAt)
    {
        throw new NotImplementedException();
    }

    public Result RevokeAdmin(EmployeeId actorId)
    {
        throw new NotImplementedException();
    }

    public Result Deactivate()
    {
        throw new NotImplementedException();
    }

    public Result Activate()
    {
        throw new NotImplementedException();
    }

    void ApplyResolvedRole(SystemRole role)
    {
        throw new NotImplementedException();
    }
}
