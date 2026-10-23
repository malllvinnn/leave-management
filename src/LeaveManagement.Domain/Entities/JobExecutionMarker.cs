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
        if (string.IsNullOrWhiteSpace(jobName))
        {
            var failureResult = Result<JobExecutionMarker>.Fail("Job name cannot be empty");

            return failureResult;
        }

        if (string.IsNullOrWhiteSpace(periodKey))
        {
            var failureResult = Result<JobExecutionMarker>.Fail("Period key cannot be empty");

            return failureResult;
        }

        var normalizedJobName = jobName.Trim();
        var normalizedPeriodKey = periodKey.Trim();

        if (normalizedJobName.Length > 100)
        {
            var failureResult = Result<JobExecutionMarker>.Fail("Job name cannot exceed 100 characters");

            return failureResult;
        }

        if (normalizedPeriodKey.Length > 50)
        {
            var failureResult = Result<JobExecutionMarker>.Fail("Period key cannot exceed 50 characters");

            return failureResult;
        }

        var utcExecutedAt = executedAt.ToUniversalTime();

        var jobExecutionMarker = new JobExecutionMarker(
            id: Guid.NewGuid(),
            jobName: normalizedJobName,
            periodKey: normalizedPeriodKey,
            executedAt: utcExecutedAt
        );

        var successResult = Result<JobExecutionMarker>.Ok(jobExecutionMarker);

        return successResult;
    }
}