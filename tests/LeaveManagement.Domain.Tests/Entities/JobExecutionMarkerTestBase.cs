using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Domain.Tests.Entities;

public abstract class JobExecutionMarkerTestBase
{
    protected static Result<JobExecutionMarker> CreateMarker(
        string? jobName = "CarryOverExpiry",
        string? periodKey = "2027",
        DateTimeOffset? executedAt = null
    )
    {
        var resolvedExecutedAt = executedAt
            ?? new DateTimeOffset(
                year: 2027,
                month: 1,
                day: 1,
                hour: 0,
                minute: 0,
                second: 0,
                offset: TimeSpan.FromHours(7)
            );

        var createResult = JobExecutionMarker.Create(
            jobName: jobName,
            periodKey: periodKey,
            executedAt: resolvedExecutedAt
        );

        return createResult;
    }
}