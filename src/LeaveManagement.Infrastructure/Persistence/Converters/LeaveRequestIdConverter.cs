using LeaveManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveManagement.Infrastructure.Persistence.Converters;

public sealed class LeaveRequestIdConverter : ValueConverter<LeaveRequestId, Guid>
{
    public LeaveRequestIdConverter() :
        base(
            (id) => id.Value,
            (value) => LeaveRequestId.Create(value).Value
        )
    { }
}