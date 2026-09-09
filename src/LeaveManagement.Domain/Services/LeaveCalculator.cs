using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Services;

public sealed class LeaveCalculator
{
    public LeaveDays Calculate(
        DateRange period,
        IReadOnlyCollection<DateOnly> holidays
    )
    {
        throw new NotImplementedException();
    }
}