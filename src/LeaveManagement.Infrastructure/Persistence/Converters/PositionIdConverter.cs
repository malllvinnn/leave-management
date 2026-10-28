using LeaveManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveManagement.Infrastructure.Persistence.Converters;

public sealed class PositionIdConverter : ValueConverter<PositionId, Guid>
{
    public PositionIdConverter() :
        base(
            (id) => id.Value,
            (value) => PositionId.Create(value).Value
        )
    { }
}