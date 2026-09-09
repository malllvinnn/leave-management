using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.ValueObjects;

public readonly record struct LeaveAllocation
{
    public LeaveDays Annual { get; }
    public LeaveDays CarryOver { get; }
    public LeaveDays Total
    {
        get
        {
            return Annual.Add(CarryOver);
        }
    }

    private LeaveAllocation(LeaveDays annual, LeaveDays carryOver)
    {
        Annual = annual;
        CarryOver = carryOver;
    }

    public static Result<LeaveAllocation> Create(LeaveDays annual, LeaveDays carryOver)
    {
        var leaveAllocation = new LeaveAllocation(annual, carryOver);

        if (leaveAllocation.Total == LeaveDays.Zero)
        {
            var failureResult = Result<LeaveAllocation>.Fail("Leave allocation must contain at least one day");

            return failureResult;
        }

        var successResult = Result<LeaveAllocation>.Ok(leaveAllocation);

        return successResult;
    }
}