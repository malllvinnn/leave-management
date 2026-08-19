namespace LeaveManagement.Domain.Common;

public sealed record DateRange
{
  private const string InvalidDateRangeError = "End date cannot be earlier than start date";
  public DateOnly Start { get; }
  public DateOnly End { get; }

  private DateRange(DateOnly start, DateOnly end)
  {
    Start = start;
    End = end;
  }

  public static Result<DateRange> Create(DateOnly start, DateOnly end)
  {
    if (end < start)
    {
      var failureResult = Result<DateRange>.Fail(InvalidDateRangeError);

      return failureResult;
    }

    var dateRange = new DateRange(start, end);
    var successResult = Result<DateRange>.Ok(dateRange);

    return successResult;
  }

  public bool OverlapsWith(DateRange other)
  {
    var overlaps = Start <= other.End && other.Start <= End;

    return overlaps;
  }

  public int DurationInDays()
  {
    var duration = End.DayNumber - Start.DayNumber + 1;

    return duration;
  }
}