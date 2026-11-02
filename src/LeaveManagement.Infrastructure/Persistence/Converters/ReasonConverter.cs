using LeaveManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveManagement.Infrastructure.Persistence.Converters;

public sealed class ReasonConverter : ValueConverter<Reason, string>
{
    public ReasonConverter() :
        base(
            (reason) => reason.Value,
            (value) => Reason.Create(value).Value
        )
    { }
}