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
        if (string.IsNullOrWhiteSpace(name))
        {
            var failureResult = Result<Holiday>.Fail("Holiday name cannot be empty");

            return failureResult;
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > 150)
        {
            var failureResult = Result<Holiday>.Fail("Holiday name cannot exceed 150 characters");

            return failureResult;
        }

        var holiday = new Holiday(
            id: Guid.NewGuid(),
            date: date,
            name: normalizedName,
            source: source
        );

        var successResult = Result<Holiday>.Ok(holiday);

        return successResult;
    }
}