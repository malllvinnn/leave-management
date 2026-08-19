namespace LeaveManagement.Domain.Common;

public sealed class Result<T>
{
  public bool IsSuccess { get; }
  public bool IsFailure => !IsSuccess;
  public string Error { get; }
  public T Value => IsSuccess ? field! : throw new InvalidOperationException(Error);

  private Result(T? value, bool isSuccess, string error)
  {
    Value = value;
    IsSuccess = isSuccess;
    Error = error;
  }

  public static Result<T> Ok(T value)
  {
    var result = new Result<T>(value, true, string.Empty);

    return result;
  }

  public static Result<T> Fail(string error)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(error);

    var result = new Result<T>(default, false, error);

    return result;
  }
}