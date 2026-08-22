using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.ValueObjects;

public readonly record struct EmployeeId
{
    private const string EmptyValueError = "Employee ID cannot be empty";
    public Guid Value { get; }

    private EmployeeId(Guid value)
    {
        Value = value;
    }

    public static EmployeeId New()
    {
        var value = Guid.CreateVersion7();
        var employeeId = new EmployeeId(value);
        
        return employeeId;
    }

    public static Result<EmployeeId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            var failureResult = Result<EmployeeId>.Fail(EmptyValueError);

            return failureResult;
        }

        var employeeId = new EmployeeId(value);
        var successResult = Result<EmployeeId>.Ok(employeeId);

        return successResult;
    }

    public override string ToString()
    {
        var result = Value.ToString();
        
        return result;
    }
}