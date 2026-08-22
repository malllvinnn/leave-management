using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.ValueObjects;

public readonly record struct LeaveBalanceId
{
    private const string EmptyValueError = "Leave Balance ID cannot be empty";
    public Guid Value { get; }

    private LeaveBalanceId(Guid value)
    {
        Value = value;
    }

    public static LeaveBalanceId New()
    {
        var value = Guid.CreateVersion7();
        var leaveBalanceId = new LeaveBalanceId(value);

        return leaveBalanceId;
    }

    public static Result<LeaveBalanceId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            var failureResult = Result<LeaveBalanceId>.Fail(EmptyValueError);

            return failureResult;
        }

        var leaveBalanceId = new LeaveBalanceId(value);
        var successResult = Result<LeaveBalanceId>.Ok(leaveBalanceId);

        return successResult;
    }

    public override string ToString()
    {
        var result = Value.ToString();

        return result;
    }
}