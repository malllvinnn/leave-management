using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.Entities;

public class JobExecutionMarker
{
    public Guid Id { get; private set; }
    public string JobName { get; private set; }
    public string PeriodKey { get; private set; }
    public DateTimeOffset ExecutedAt { get; private set; }

    private JobExecutionMarker(
        Guid id,
        string jobName,
        string periodKey,
        DateTimeOffset executedAt
    )
    {
        Id = id;
        JobName = jobName;
        PeriodKey = periodKey;
        ExecutedAt = executedAt;
    }

    public static Result<JobExecutionMarker> Create(
        string? jobName,
        string? periodKey,
        DateTimeOffset executedAt
    )
    {
        throw new NotImplementedException();
    }
}