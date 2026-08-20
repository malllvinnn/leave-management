using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.ValueObjects;

public sealed record Reason
{
  private const int MaximumLength = 500;
  private const string EmptyValueError = "Reason cannot be empty";
  private const string MaximumLengthError = "Reason cannot exceed 500 characters";

  public string Value { get; }

  private Reason(string value)
  {
    Value = value;
  }

  public static Result<Reason> Create(string? value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      var failureResult = Result<Reason>.Fail(EmptyValueError);

      return failureResult;
    }

    var normalizedValue = value.Trim();

    if (normalizedValue.Length > MaximumLength)
    {
      var failureResult = Result<Reason>.Fail(MaximumLengthError);

      return failureResult;
    }

    var reason = new Reason(normalizedValue);
    var successResult = Result<Reason>.Ok(reason);

    return successResult;
  }
}