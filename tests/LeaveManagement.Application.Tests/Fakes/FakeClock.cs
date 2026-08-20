using LeaveManagement.Application.Interfaces;

namespace LeaveManagement.Application.Tests.Fakes;

public sealed class FakeClock : IClock
{
  public DateOnly Today { get; private set; }
  public DateTimeOffset UtcNow { get; private set; }

  public FakeClock(DateOnly today, DateTimeOffset utcNow)
  {
    Today = today;
    UtcNow = utcNow.ToUniversalTime();
  }

  public void SetToday(DateOnly today)
  {
    Today = today;
  }

  public void SetUtcNow(DateTimeOffset utcNow)
  {
    UtcNow = utcNow.ToUniversalTime();
  }
}