using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Entities;

public class ApprovalDecision
{
    public Guid Id { get; private set; }
    public LeaveRequestId LeaveRequestId { get; private set; }
    public EmployeeId ActorId { get; private set; }
    public DecisionType Type { get; private set; }
    public ApprovalStage Stage { get; private set; }
    public Reason? Reason { get; private set; }
    public EmployeeId? BypassedApproverId { get; private set; }
    public DecisionType? OverrideOutcome { get; private set; }
    public DateTimeOffset DecidedAt { get; private set; }

    private ApprovalDecision(
        Guid id,
        LeaveRequestId requestId,
        EmployeeId actorId,
        DecisionType type,
        ApprovalStage stage,
        Reason? reason,
        EmployeeId? bypassedApproverId,
        DecisionType? overrideOutcome,
        DateTimeOffset decidedAt
    )
    {
        Id = id;
        LeaveRequestId = requestId;
        ActorId = actorId;
        Type = type;
        Stage = stage;
        Reason = reason;
        BypassedApproverId = bypassedApproverId;
        OverrideOutcome = overrideOutcome;
        DecidedAt = decidedAt.ToUniversalTime();
    }

    public static Result<ApprovalDecision> Create(
        LeaveRequestId requestId,
        EmployeeId actorId,
        DecisionType type,
        ApprovalStage stage,
        Reason? reason,
        EmployeeId? bypassedApproverId,
        DecisionType? overrideOutcome,
        DateTimeOffset decidedAt
    )
    {
        var id = Guid.CreateVersion7();

        var approvalDecision = new ApprovalDecision(
            id: id,
            requestId: requestId,
            actorId: actorId,
            type: type,
            stage: stage,
            reason: reason,
            bypassedApproverId: bypassedApproverId,
            overrideOutcome: overrideOutcome,
            decidedAt: decidedAt
        );

        var successResult = Result<ApprovalDecision>.Ok(approvalDecision);

        return successResult;
    }
}