using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Entities;

public class QuotaAdjustment
{
    public Guid Id { get; private set; }
    public LeaveBalanceId LeaveBalanceId { get; private set; }
    public int Days { get; private set; }
    public Reason Reason { get; private set; }
    public EmployeeId AdjustedBy { get; private set; }
    public DateTimeOffset AdjustedAt { get; private set; }

    private QuotaAdjustment(
        Guid id,
        LeaveBalanceId leaveBalanceId,
        int days,
        Reason reason,
        EmployeeId adjustedBy,
        DateTimeOffset adjustedAt
    )
    {
        Id = id;
        LeaveBalanceId = leaveBalanceId;
        Days = days;
        Reason = reason;
        AdjustedBy = adjustedBy;
        AdjustedAt = adjustedAt;
    }

    public static Result<QuotaAdjustment> Create(
        LeaveBalanceId leaveBalanceId,
        int days,
        Reason? reason,
        EmployeeId adjustedBy,
        DateTimeOffset adjustedAt
    )
    {
        if (days == 0)
        {
            var failureResult = Result<QuotaAdjustment>.Fail("Quota adjustment days cannot be zero");

            return failureResult;
        }

        if (reason is null)
        {
            var failureResult = Result<QuotaAdjustment>.Fail("Quota adjustment reason cannot be empty");

            return failureResult;
        }

        var normalizedAdjustedAt = adjustedAt.ToUniversalTime();

        var quotaAdjustment = new QuotaAdjustment(
            id: Guid.CreateVersion7(),
            leaveBalanceId: leaveBalanceId,
            days: days,
            reason: reason,
            adjustedBy: adjustedBy,
            adjustedAt: normalizedAdjustedAt
        );

        var successResult = Result<QuotaAdjustment>.Ok(quotaAdjustment);

        return successResult;
    }
}