using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class ApprovalDecisionTests
{
    private LeaveRequestId _requestId;
    private EmployeeId _actorId;
    private Reason _validReason;
    private DateTimeOffset _decidedAtPlusSeven;

    [SetUp]
    public void SetUp()
    {
        _requestId = LeaveRequestId.New();
        _actorId = EmployeeId.New();
        _validReason = Reason.Create("Valid reason").Value;

        _decidedAtPlusSeven = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 2,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
    );
    }

    [TestCase(DecisionType.Approve, ApprovalStage.First, false)]
    [TestCase(DecisionType.Approve, ApprovalStage.First, true)]
    [TestCase(DecisionType.Approve, ApprovalStage.Final, false)]
    [TestCase(DecisionType.Approve, ApprovalStage.Final, true)]
    [TestCase(DecisionType.Cancel, ApprovalStage.First, false)]
    [TestCase(DecisionType.Cancel, ApprovalStage.First, true)]
    [TestCase(DecisionType.Cancel, ApprovalStage.Final, false)]
    [TestCase(DecisionType.Cancel, ApprovalStage.Final, true)]
    public void Create_WithApproveOrCancelAndOptionalReason_ReturnsSuccessfulResultWithCompleteState(
        DecisionType type,
        ApprovalStage stage,
        bool hasReason
    )
    {
        // Arrange
        var requestId = _requestId;
        Reason? reason = hasReason ? _validReason : null;

        // Act
        var result = ApprovalDecision.Create(
            requestId: requestId,
            actorId: _actorId,
            type: type,
            stage: stage,
            reason: reason,
            bypassedApproverId: null,
            overrideOutcome: null,
            decidedAt: _decidedAtPlusSeven
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var decision = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(decision.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(decision.LeaveRequestId, Is.EqualTo(requestId));
            Assert.That(decision.ActorId, Is.EqualTo(_actorId));
            Assert.That(decision.Type, Is.EqualTo(type));
            Assert.That(decision.Stage, Is.EqualTo(stage));
            Assert.That(decision.Reason, Is.EqualTo(reason));
            Assert.That(decision.BypassedApproverId, Is.Null);
            Assert.That(decision.OverrideOutcome, Is.Null);
            Assert.That(decision.DecidedAt, Is.EqualTo(_decidedAtPlusSeven.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

    [TestCase(ApprovalStage.First)]
    [TestCase(ApprovalStage.Final)]
    public void Create_WithRejectAndValidReason_ReturnsSuccessfulResultWithCompleteState(ApprovalStage stage)
    {
        // Arrange
        var type = DecisionType.Reject;
        var reason = _validReason;

        // Act
        var result = ApprovalDecision.Create(
            requestId: _requestId,
            actorId: _actorId,
            type: type,
            stage: stage,
            reason: reason,
            bypassedApproverId: null,
            overrideOutcome: null,
            decidedAt: _decidedAtPlusSeven
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var decision = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(decision.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(decision.LeaveRequestId, Is.EqualTo(_requestId));
            Assert.That(decision.ActorId, Is.EqualTo(_actorId));
            Assert.That(decision.Type, Is.EqualTo(type));
            Assert.That(decision.Stage, Is.EqualTo(stage));
            Assert.That(decision.Reason, Is.EqualTo(reason));
            Assert.That(decision.BypassedApproverId, Is.Null);
            Assert.That(decision.OverrideOutcome, Is.Null);
            Assert.That(decision.DecidedAt, Is.EqualTo(_decidedAtPlusSeven.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

    [TestCase(ApprovalStage.First)]
    [TestCase(ApprovalStage.Final)]
    public void Create_WithRejectAndNullReason_ReturnsFailedResultWithError(ApprovalStage stage)
    {
        // Arrange
        var type = DecisionType.Reject;
        Reason? reason = null;

        // Act
        var result = ApprovalDecision.Create(
            requestId: _requestId,
            actorId: _actorId,
            type: type,
            stage: stage,
            reason: reason,
            bypassedApproverId: null,
            overrideOutcome: null,
            decidedAt: _decidedAtPlusSeven
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Rejection reason cannot be empty"));
        });
    }

    [TestCase(DecisionType.Approve)]
    [TestCase(DecisionType.Reject)]
    [TestCase(DecisionType.Cancel)]
    public void Create_WithNonOverrideTypeAndOverrideOutcome_ReturnsFailedResultWithError(DecisionType type)
    {
        // Arrange
        var stage = ApprovalStage.First;
        var overrideOutcome = DecisionType.Approve;

        // Act
        var result = ApprovalDecision.Create(
            requestId: _requestId,
            actorId: _actorId,
            type: type,
            stage: stage,
            reason: _validReason,
            bypassedApproverId: null,
            overrideOutcome: overrideOutcome,
            decidedAt: _decidedAtPlusSeven
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Override outcome is only allowed for override decisions"));
        });
    }

    [TestCase(DecisionType.Approve)]
    [TestCase(DecisionType.Reject)]
    [TestCase(DecisionType.Cancel)]
    public void Create_WithNonOverrideTypeAndBypassedApprover_ReturnsFailedResultWithError(DecisionType type)
    {
        // Arrange
        var stage = ApprovalStage.First;
        var bypassedApproverId = EmployeeId.New();

        // Act
        var result = ApprovalDecision.Create(
            requestId: _requestId,
            actorId: _actorId,
            type: type,
            stage: stage,
            reason: _validReason,
            bypassedApproverId: bypassedApproverId,
            overrideOutcome: null,
            decidedAt: _decidedAtPlusSeven
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Bypassed approver is only allowed for override decisions"));
        });
    }

    [TestCase(DecisionType.Approve, ApprovalStage.First, false)]
    [TestCase(DecisionType.Reject, ApprovalStage.First, false)]
    [TestCase(DecisionType.Approve, ApprovalStage.Final, false)]
    [TestCase(DecisionType.Reject, ApprovalStage.Final, false)]
    [TestCase(DecisionType.Approve, ApprovalStage.First, true)]
    [TestCase(DecisionType.Reject, ApprovalStage.First, true)]
    [TestCase(DecisionType.Approve, ApprovalStage.Final, true)]
    [TestCase(DecisionType.Reject, ApprovalStage.Final, true)]
    public void Create_WithValidOverrideContext_ReturnsSuccessfulResultWithCompleteState(
        DecisionType outcome,
        ApprovalStage stage,
        bool hasBypassedApprover
    )
    {
        // Arrange
        var type = DecisionType.Override;
        var reason = Reason.Create("Valid override reason").Value;
        EmployeeId? bypassedApproverId = hasBypassedApprover ? EmployeeId.New() : null;

        // Act
        var result = ApprovalDecision.Create(
            requestId: _requestId,
            actorId: _actorId,
            type: type,
            stage: stage,
            reason: reason,
            bypassedApproverId: bypassedApproverId,
            overrideOutcome: outcome,
            decidedAt: _decidedAtPlusSeven
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var decision = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(decision.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(decision.LeaveRequestId, Is.EqualTo(_requestId));
            Assert.That(decision.ActorId, Is.EqualTo(_actorId));
            Assert.That(decision.Type, Is.EqualTo(type));
            Assert.That(decision.Stage, Is.EqualTo(stage));
            Assert.That(decision.Reason, Is.EqualTo(reason));
            Assert.That(decision.BypassedApproverId, Is.EqualTo(bypassedApproverId));
            Assert.That(decision.OverrideOutcome, Is.EqualTo(outcome));
            Assert.That(decision.DecidedAt, Is.EqualTo(_decidedAtPlusSeven.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }
}
