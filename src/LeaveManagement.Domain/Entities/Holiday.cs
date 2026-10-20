using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Domain.Entities;

public class Holiday
{
    public Guid Id { get; private set; }
    public DateOnly Date { get; private set; }
    public string Name { get; private set; }
    public HolidaySource Source { get; private set; }

    private Holiday(
        Guid id,
        DateOnly date,
        string name,
        HolidaySource source
    )
    {
        Id = id;
        Date = date;
        Name = name;
        Source = source;
    }

    public static Result<Holiday> Create(
        DateOnly date,
        string? name,
        HolidaySource source
    )
    {
        throw new NotImplementedException();
    }
}