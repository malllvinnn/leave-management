using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Services;

public sealed class LeaveCalculator
{
    public LeaveDays Calculate(
        DateRange period,
        IReadOnlyCollection<DateOnly> holidays
    )
    {
        if (period is null)
        {
            throw new ArgumentNullException(nameof(period));
        }

        if (holidays is null)
        {
            throw new ArgumentNullException(nameof(holidays));
        }

        throw new NotImplementedException();
    }
}