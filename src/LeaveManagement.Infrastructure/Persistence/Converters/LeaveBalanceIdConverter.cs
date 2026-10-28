using LeaveManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveManagement.Infrastructure.Persistence.Converters;

public sealed class LeaveBalanceIdConverter : ValueConverter<LeaveBalanceId, Guid>
{
    public LeaveBalanceIdConverter() :
        base(
            (id) => id.Value,
            (value) => LeaveBalanceId.Create(value).Value
        )
    { }
}