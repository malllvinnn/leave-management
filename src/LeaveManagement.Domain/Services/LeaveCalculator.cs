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

        int workingDaysCount = 0;

        for (var date = period.Start; date <= period.End; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                continue; // Skip weekends
            }

            workingDaysCount++;
        }

        var result = LeaveDays.Create(workingDaysCount).Value;

        return result;
    }
}