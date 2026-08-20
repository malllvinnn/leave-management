using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.ValueObjects;

public readonly record struct LeaveDays
{
  private const string NegativeValueError = "Leave days cannot be negative";
  public int Value { get; }

  private LeaveDays(int value)
  {
    Value = value;
  }

  public static readonly LeaveDays Zero = new LeaveDays(0);

  public static Result<LeaveDays> Create(int value)
  {
    if (value < 0)
    {
      var failureResult = Result<LeaveDays>.Fail(NegativeValueError);

      return failureResult;
    }

    var leaveDays = new LeaveDays(value);
    var successResult = Result<LeaveDays>.Ok(leaveDays);

    return successResult;
  }

  public LeaveDays Add(LeaveDays other)
  {
    int totalValue = checked(Value + other.Value);
    var result = new LeaveDays(totalValue);

    return result;
  }

  public Result<LeaveDays> Subtract(LeaveDays other)
  {
    int remainingValue = Value - other.Value;
    var result = Create(remainingValue);

    return result;
  }

  public override string ToString()
  {
    var result = $"{Value} days";

    return result;
  }
}
