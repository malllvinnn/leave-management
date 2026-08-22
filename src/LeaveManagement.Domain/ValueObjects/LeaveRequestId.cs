using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.ValueObjects;

public readonly record struct LeaveRequestId
{
    private const string EmptyValueError = "Leave Request ID cannot be empty";
    public Guid Value { get; }

    private LeaveRequestId(Guid value)
    {
        Value = value;
    }

    public static LeaveRequestId New()
    {
        var value = Guid.CreateVersion7();
        var leaveRequestId = new LeaveRequestId(value);

        return leaveRequestId;
    }

    public static Result<LeaveRequestId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            var failureResult = Result<LeaveRequestId>.Fail(EmptyValueError);

            return failureResult;
        }

        var leaveRequestId = new LeaveRequestId(value);
        var successResult = Result<LeaveRequestId>.Ok(leaveRequestId);

        return successResult;
    }

    public override string ToString()
    {
        var result = Value.ToString();

        return result;
    }
}