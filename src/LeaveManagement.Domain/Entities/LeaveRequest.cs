using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Entities;

public class LeaveRequest
{
    public LeaveRequestId Id { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public DateRange Period { get; private set; }
    public LeaveDays WorkingDays { get; private set; }
    public LeaveAllocation Allocation { get; private set; }
    public LeaveType Type { get; private set; }
    public Reason Reason { get; private set; }
    public LeaveStatus Status { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }

    public IReadOnlyList<ApprovalDecision> Decisions
    {
        get
        {
            return _decisions.AsReadOnly();
        }
    }

    private readonly List<ApprovalDecision> _decisions = [];

    private LeaveRequest(
        LeaveRequestId id,
        EmployeeId employeeId,
        DateRange period,
        LeaveDays workingDays,
        LeaveAllocation allocation,
        LeaveType type,
        Reason reason,
        DateTimeOffset submittedAt
    )
    {
        Id = id;
        EmployeeId = employeeId;
        Period = period;
        WorkingDays = workingDays;
        Allocation = allocation;
        Type = type;
        Reason = reason;
        SubmittedAt = submittedAt.ToUniversalTime();

        Status = LeaveStatus.Pending;
    }

    public static Result<LeaveRequest> Create(
        EmployeeId employeeId,
        DateRange period,
        LeaveDays workingDays,
        LeaveAllocation allocation,
        LeaveType type,
        Reason reason,
        DateOnly today,
        DateTimeOffset submittedAt
    )
    {
        if (period.Start.Year != period.End.Year)
        {
            var failureResult = Result<LeaveRequest>.Fail("Leave request must be within a single calendar year");

            return failureResult;
        }

        if (period.Start.Year != today.Year)
        {
            var failureResult = Result<LeaveRequest>.Fail("Leave request must be within the current calendar year");

            return failureResult;
        }

        if (allocation.Total != workingDays)
        {
            var failureResult = Result<LeaveRequest>.Fail("Leave allocation must equal the working days of the request");

            return failureResult;
        }

        var newId = LeaveRequestId.New();

        var leaveRequest = new LeaveRequest(
            id: newId,
            employeeId: employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: type,
            reason: reason,
            submittedAt: submittedAt
        );

        var successResult = Result<LeaveRequest>.Ok(leaveRequest);

        return successResult;
    }

    public Result ApproveFirstStage(
        EmployeeId approverId,
        bool requiresFinalApproval,
        Reason? reason,
        DateTimeOffset now
    )
    {
        if (approverId == EmployeeId)
        {
            var failureResult = Result.Fail("Approver cannot decide their own leave request");

            return failureResult;
        }

        if (Status is not LeaveStatus.Pending)
        {
            var failureResult = Result.Fail("Leave request is not awaiting first stage approval");

            return failureResult;
        }

        if (requiresFinalApproval && WorkingDays.Value <= 5)
        {
            var failureResult = Result.Fail("Final approval cannot be required for requests of 5 working days or fewer");

            return failureResult;
        }

        var targetStatus = requiresFinalApproval
            ? LeaveStatus.PendingFinalApproval
            : LeaveStatus.Approved;

        var approvalDecision = ApprovalDecision.Create(
            requestId: Id,
            actorId: approverId,
            type: DecisionType.Approve,
            stage: ApprovalStage.First,
            reason: reason,
            bypassedApproverId: null,
            overrideOutcome: null,
            decidedAt: now
        );

        if (approvalDecision.IsFailure)
        {
            var failureResult = Result.Fail(approvalDecision.Error);

            return failureResult;
        }

        var decision = approvalDecision.Value;

        _decisions.Add(decision);
        Status = targetStatus;

        var successResult = Result.Ok();

        return successResult;
    }

    public Result ApproveFinalStage(
        EmployeeId approverId,
        Reason? reason,
        DateTimeOffset now
    )
    {
        if (approverId == EmployeeId)
        {
            var failureResult = Result.Fail("Approver cannot decide their own leave request");

            return failureResult;
        }

        if (Status is not LeaveStatus.PendingFinalApproval)
        {
            var failureResult = Result.Fail("Leave request is not awaiting final approval");

            return failureResult;
        }

        var approvalDecision = ApprovalDecision.Create(
            requestId: Id,
            actorId: approverId,
            type: DecisionType.Approve,
            stage: ApprovalStage.Final,
            reason: reason,
            bypassedApproverId: null,
            overrideOutcome: null,
            decidedAt: now
        );

        if (approvalDecision.IsFailure)
        {
            var failureResult = Result.Fail(approvalDecision.Error);

            return failureResult;
        }

        var decision = approvalDecision.Value;

        _decisions.Add(decision);
        Status = LeaveStatus.Approved;

        var successResult = Result.Ok();

        return successResult;
    }

    public Result Override(
        EmployeeId actorId,
        EmployeeId? bypassedApproverId,
        DecisionType outcome,
        Reason? reason,
        DateTimeOffset now
    )
    {
        if (actorId == EmployeeId)
        {
            var failureResult = Result.Fail("Approver cannot decide their own leave request");

            return failureResult;
        }

        if (Status is not LeaveStatus.Pending and not LeaveStatus.PendingFinalApproval)
        {
            var failureResult = Result.Fail("Leave request has already reached a final decision");

            return failureResult;
        }

        var decisionStage = Status is LeaveStatus.Pending
            ? ApprovalStage.First
            : ApprovalStage.Final;

        var approvalDecision = ApprovalDecision.Create(
            requestId: Id,
            actorId: actorId,
            type: DecisionType.Override,
            stage: decisionStage,
            reason: reason,
            bypassedApproverId: bypassedApproverId,
            overrideOutcome: outcome,
            decidedAt: now
        );

        if (approvalDecision.IsFailure)
        {
            var failureResult = Result.Fail(approvalDecision.Error);

            return failureResult;
        }

        var targetStatus = outcome == DecisionType.Approve
            ? LeaveStatus.Approved
            : LeaveStatus.Rejected;

        var decision = approvalDecision.Value;
        _decisions.Add(decision);
        Status = targetStatus;

        var successResult = Result.Ok();

        return successResult;
    }

    public Result Reject(
        EmployeeId actorId,
        Reason? reason,
        DateTimeOffset now
    )
    {
        if (actorId == EmployeeId)
        {
            var failureResult = Result.Fail("Approver cannot decide their own leave request");

            return failureResult;
        }

        if (Status is not LeaveStatus.Pending and not LeaveStatus.PendingFinalApproval)
        {
            var failureResult = Result.Fail("Leave request has already reached a final decision");

            return failureResult;
        }

        var decisionStage = Status is LeaveStatus.Pending
            ? ApprovalStage.First
            : ApprovalStage.Final;

        var approvalDecision = ApprovalDecision.Create(
            requestId: Id,
            actorId: actorId,
            type: DecisionType.Reject,
            stage: decisionStage,
            reason: reason,
            bypassedApproverId: null,
            overrideOutcome: null,
            decidedAt: now
        );

        if (approvalDecision.IsFailure)
        {
            var failureResult = Result.Fail(approvalDecision.Error);

            return failureResult;
        }

        var decision = approvalDecision.Value;
        _decisions.Add(decision);
        Status = LeaveStatus.Rejected;

        var successResult = Result.Ok();

        return successResult;
    }

    public Result Cancel(
        EmployeeId actorId,
        DateOnly today,
        Reason? reason,
        DateTimeOffset now
    )
    {
        if (Status is not LeaveStatus.Pending and not LeaveStatus.PendingFinalApproval and not LeaveStatus.Approved)
        {
            var failureResult = Result.Fail("Leave request has already reached a final decision");

            return failureResult;
        }

        if (today >= Period.Start)
        {
            var failureResult = Result.Fail("Leave request cannot be cancelled on or after its start date");

            return failureResult;
        }

        var decisionStage = Status switch
        {
            LeaveStatus.Pending => ApprovalStage.First,
            LeaveStatus.PendingFinalApproval => ApprovalStage.Final,
            LeaveStatus.Approved => _decisions[^1].Stage,
            _ => throw new InvalidOperationException("Unexpected leave request status for cancellation")
        };

        var approvalDecision = ApprovalDecision.Create(
            requestId: Id,
            actorId: actorId,
            type: DecisionType.Cancel,
            stage: decisionStage,
            reason: reason,
            bypassedApproverId: null,
            overrideOutcome: null,
            decidedAt: now
        );

        if (approvalDecision.IsFailure)
        {
            var failureResult = Result.Fail(approvalDecision.Error);

            return failureResult;
        }

        var decision = approvalDecision.Value;
        _decisions.Add(decision);
        Status = LeaveStatus.Cancelled;

        var successResult = Result.Ok();

        return successResult;
    }
}