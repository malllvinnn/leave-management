using LeaveManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveManagement.Infrastructure.Persistence.Converters;

public sealed class EmailConverter : ValueConverter<Email, string>
{
    public EmailConverter() :
        base(
            (email) => email.Value,
            (value) => Email.Create(value).Value
        )
    { }
}