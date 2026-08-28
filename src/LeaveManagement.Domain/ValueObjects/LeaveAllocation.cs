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

    public Result<LeaveAllocation> Create(LeaveDays annual, LeaveDays carryOver)
    {
        throw new NotImplementedException();
    }
}