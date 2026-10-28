using LeaveManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveManagement.Infrastructure.Persistence.Converters;

public sealed class LeaveDaysConverter : ValueConverter<LeaveDays, int>
{
    public LeaveDaysConverter() :
        base(
            (days) => days.Value,
            (value) => LeaveDays.Create(value).Value
        )
    { }
}