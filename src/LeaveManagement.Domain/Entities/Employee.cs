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

        if (email is null)
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
        if (managerId == Id)
        {
            var failureResult = Result.Fail("Employee cannot be assigned as their own manager");

            return failureResult;
        }

        if (managerId is null)
        {
            ManagerId = null;
        }

        ManagerId = managerId;

        var successResult = Result.Ok();

        return successResult;
    }

    public Result AssignPosition(PositionId? positionId)
    {
        if (positionId is null)
        {
            PositionId = null;
        }

        PositionId = positionId;

        var successResult = Result.Ok();

        return successResult;
    }

    public Result GrantAdmin(EmployeeId actorId, DateTimeOffset grantedAt)
    {
        if (actorId == Id)
        {
            var failureResult = Result.Fail("Employee cannot grant administrator access to themselves");

            return failureResult;
        }

        if (AdminGrantedAt is not null)
        {
            var failureResult = Result.Fail("Administrator access has already been granted to the employee");

            return failureResult;
        }

        AdminGrantedAt = grantedAt.ToUniversalTime();

        var successResult = Result.Ok();

        return successResult;
    }

    public Result RevokeAdmin(EmployeeId actorId)
    {
        if (actorId == Id)
        {
            var failureResult = Result.Fail("Employee cannot revoke their own administrator access");

            return failureResult;
        }

        if (AdminGrantedAt is null)
        {
            var failureResult = Result.Fail("Administrator access has not been granted to the employee");

            return failureResult;
        }

        AdminGrantedAt = null;

        var successResult = Result.Ok();

        return successResult;
    }

    public Result Deactivate()
    {
        if (IsActive is false)
        {
            var failureResult = Result.Fail("Employee is already inactive");

            return failureResult;
        }

        IsActive = false;

        var successResult = Result.Ok();

        return successResult;
    }

    public Result Activate()
    {
        if (IsActive is true)
        {
            var failureResult = Result.Fail("Employee is already active");

            return failureResult;
        }

        IsActive = true;

        var successResult = Result.Ok();

        return successResult;
    }

    void ApplyResolvedRole(SystemRole role)
    {
        throw new NotImplementedException();
    }
}
