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
        if (string.IsNullOrWhiteSpace(fullName))
        {
            var failureResult = Result<Employee>.Fail("Employee full name cannot be empty");

            return failureResult;
        }

        var normalizedFullName = fullName.Trim();

        if (normalizedFullName.Length > 200)
        {
            var failureResult = Result<Employee>.Fail("Employee full name cannot exceed 200 characters");

            return failureResult;
        }

        if (email == null)
        {
            var failureResult = Result<Employee>.Fail("Employee email cannot be empty");

            return failureResult;
        }

        var employeeId = EmployeeId.New();

        var employee = new Employee(
            id: employeeId,
            fullName: normalizedFullName,
            email: email,
            hireDate: hireDate,
            managerId: managerId,
            positionId: positionId
        );

        var successResult = Result<Employee>.Ok(employee);

        return successResult;
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
