namespace LeaveManagement.Domain.Common;

public sealed class Result
{
  public bool IsSuccess { get; }
  public bool IsFailure => !IsSuccess;
  public string Error { get; }
  private Result(bool isSuccess, string error)
  {
    IsSuccess = isSuccess;
    Error = error;
  }

  public static Result Ok()
  {
    var result = new Result(true, string.Empty);

    return result;
  }

  public static Result Fail(string error)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(error);

    var result = new Result(false, error);

    return result;
  }
}