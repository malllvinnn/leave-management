namespace LeaveManagement.Application.Interfaces;

public interface IClock
{
  DateOnly Today { get; }
  DateTimeOffset UtcNow { get; }
}