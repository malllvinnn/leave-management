using LeaveManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveManagement.Infrastructure.Persistence.Converters;

public sealed class EmployeeIdConverter : ValueConverter<EmployeeId, Guid>
{
    public EmployeeIdConverter() :
        base(
            (id) => id.Value,
            (value) => EmployeeId.Create(value).Value
        )
    { }
}