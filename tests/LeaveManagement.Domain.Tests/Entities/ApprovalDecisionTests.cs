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

        var request = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(request.LeaveRequestId, Is.EqualTo(requestId));
            Assert.That(request.ActorId, Is.EqualTo(_actorId));
            Assert.That(request.Type, Is.EqualTo(type));
            Assert.That(request.Stage, Is.EqualTo(stage));
            Assert.That(request.Reason, Is.EqualTo(reason));
            Assert.That(request.BypassedApproverId, Is.Null);
            Assert.That(request.OverrideOutcome, Is.Null);
            Assert.That(request.DecidedAt, Is.EqualTo(_decidedAtPlusSeven.ToUniversalTime()));
            Assert.That(request.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

}