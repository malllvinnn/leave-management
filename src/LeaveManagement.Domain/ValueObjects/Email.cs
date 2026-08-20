using LeaveManagement.Domain.Common;
using System.Net.Mail;

namespace LeaveManagement.Domain.ValueObjects;

public sealed record Email
{
  private const int MaximumLength = 255;
  private const string EmptyValueError = "Email cannot be empty";
  private const string InvalidFormatError = "Email format is invalid";
  private const string MaximumLengthError = "Email cannot exceed 255 characters";

  public string Value { get; }

  private Email(string value)
  {
    Value = value;
  }

  public static Result<Email> Create(string? value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      var failureResult = Result<Email>.Fail(EmptyValueError);

      return failureResult;
    }

    var normalizedValue = value.Trim().ToLowerInvariant();

    if (normalizedValue.Length > MaximumLength)
    {
      var failureResult = Result<Email>.Fail(MaximumLengthError);

      return failureResult;
    }

    var hasValidFormat = MailAddress.TryCreate(normalizedValue, out var mailAddress) && string.Equals(
      mailAddress.Address,
      normalizedValue,
      StringComparison.Ordinal);

    if (!hasValidFormat)
    {
      var failureResult = Result<Email>.Fail(InvalidFormatError);

      return failureResult;
    }

    var email = new Email(normalizedValue);
    var successResult = Result<Email>.Ok(email);

    return successResult;
  }
}
