using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class LeaveRequestOverrideTests : LeaveRequestTestBase
{

    [TestCase(ApprovalStage.First, DecisionType.Approve, false)]
    [TestCase(ApprovalStage.First, DecisionType.Reject, false)]
    [TestCase(ApprovalStage.Final, DecisionType.Approve, false)]
    [TestCase(ApprovalStage.Final, DecisionType.Reject, false)]
    [TestCase(ApprovalStage.First, DecisionType.Approve, true)]
    [TestCase(ApprovalStage.First, DecisionType.Reject, true)]
    [TestCase(ApprovalStage.Final, DecisionType.Approve, true)]
    [TestCase(ApprovalStage.Final, DecisionType.Reject, true)]
    public void Override_WithValidOutcome_ReturnsExpectedStatusWithOverrideDecision(
        ApprovalStage stage,
        DecisionType outcome,
        bool hasBypassedApprover
    )
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(stage);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        if (stage is ApprovalStage.Final)
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: true,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }

        EmployeeId? bypassedApproverId = hasBypassedApprover ? EmployeeId.New() : null;

        // Act
        var result = request.Override(
            actorId: _overrideActorId,
            bypassedApproverId: bypassedApproverId,
            outcome: outcome,
            reason: _validOverrideReason,
            now: _overrideAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(request.Decisions, Has.Count.EqualTo(
            stage is ApprovalStage.First ? 1 : 2
        ));

        var decisionIndex = stage is ApprovalStage.First ? 0 : 1;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.Status, Is.EqualTo(
                outcome is DecisionType.Approve ? LeaveStatus.Approved : LeaveStatus.Rejected
            ));
            Assert.That(request.Decisions[decisionIndex].Type, Is.EqualTo(DecisionType.Override));
            Assert.That(request.Decisions[decisionIndex].Stage, Is.EqualTo(stage));
            Assert.That(request.Decisions[decisionIndex].OverrideOutcome, Is.EqualTo(outcome));
            Assert.That(request.Decisions[decisionIndex].BypassedApproverId, Is.EqualTo(bypassedApproverId));
            Assert.That(request.Decisions[decisionIndex].Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(request.Decisions[decisionIndex].LeaveRequestId, Is.EqualTo(request.Id));
            Assert.That(request.Decisions[decisionIndex].ActorId, Is.EqualTo(_overrideActorId));
            Assert.That(request.Decisions[decisionIndex].Reason, Is.EqualTo(_validOverrideReason));
            Assert.That(request.Decisions[decisionIndex].DecidedAt, Is.EqualTo(_overrideAt.ToUniversalTime()));
            Assert.That(request.Decisions[decisionIndex].DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

    [TestCase(ApprovalStage.First, null)]
    [TestCase(ApprovalStage.First, "123456789")]
    [TestCase(ApprovalStage.Final, null)]
    [TestCase(ApprovalStage.Final, "123456789")]
    public void Override_WithMissingOrShortReason_ReturnsFailedResultWithoutChangingRequest(
        ApprovalStage stage,
        string? rawReason
    )
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(stage);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        if (stage is ApprovalStage.Final)
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: true,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }

        Guid? firstDecisionId = stage is ApprovalStage.Final ? request.Decisions[0].Id : null;

        Reason? reason = rawReason is null
            ? null
            : Reason.Create(rawReason).Value;

        // Act
        var result = request.Override(
            actorId: _overrideActorId,
            bypassedApproverId: _validBypassedId,
            outcome: DecisionType.Approve,
            reason: reason,
            now: _overrideAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Override reason must be at least 10 characters"));

            Assert.That(request.Status, Is.EqualTo(
                stage is ApprovalStage.First
                    ? LeaveStatus.Pending
                    : LeaveStatus.PendingFinalApproval
            ));

            Assert.That(request.Decisions, Has.Count.EqualTo(
                stage is ApprovalStage.First ? 0 : 1
            ));

            if (stage is ApprovalStage.Final)
            {
                Assert.That(request.Decisions[0].Id, Is.EqualTo(firstDecisionId));
            }
        });
    }

    [TestCase(ApprovalStage.First, DecisionType.Cancel)]
    [TestCase(ApprovalStage.First, DecisionType.Override)]
    [TestCase(ApprovalStage.Final, DecisionType.Cancel)]
    [TestCase(ApprovalStage.Final, DecisionType.Override)]
    public void Override_WithInvalidOutcome_ReturnsFailedResultWithoutChangingRequest(
        ApprovalStage stage,
        DecisionType outcome
    )
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(stage);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        if (stage is ApprovalStage.Final)
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: true,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }

        Guid? firstDecisionId = stage is ApprovalStage.Final ? request.Decisions[0].Id : null;

        // Act
        var result = request.Override(
            actorId: _overrideActorId,
            bypassedApproverId: _validBypassedId,
            outcome: outcome,
            reason: _validOverrideReason,
            now: _overrideAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Override outcome must be approve or reject"));

            Assert.That(request.Status, Is.EqualTo(
                stage is ApprovalStage.First
                    ? LeaveStatus.Pending
                    : LeaveStatus.PendingFinalApproval
            ));

            Assert.That(request.Decisions, Has.Count.EqualTo(
                stage is ApprovalStage.First ? 0 : 1
            ));

            if (stage is ApprovalStage.Final)
            {
                Assert.That(request.Decisions[0].Id, Is.EqualTo(firstDecisionId));
            }
        });
    }

    [TestCase(ApprovalStage.First)]
    [TestCase(ApprovalStage.Final)]
    public void Override_ByRequester_ReturnsFailedResultWithoutChangingRequest(
        ApprovalStage stage
    )
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(stage);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        if (stage is ApprovalStage.Final)
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: true,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }

        Guid? firstDecisionId = stage is ApprovalStage.Final ? request.Decisions[0].Id : null;

        // Act
        var result = request.Override(
            actorId: request.EmployeeId,
            bypassedApproverId: _validBypassedId,
            outcome: DecisionType.Approve,
            reason: _validOverrideReason,
            now: _overrideAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Approver cannot decide their own leave request"));

            Assert.That(request.Status, Is.EqualTo(
                stage is ApprovalStage.First
                    ? LeaveStatus.Pending
                    : LeaveStatus.PendingFinalApproval
            ));

            Assert.That(request.Decisions, Has.Count.EqualTo(
                stage is ApprovalStage.First ? 0 : 1
            ));

            if (stage is ApprovalStage.Final)
            {
                Assert.That(request.Decisions[0].Id, Is.EqualTo(firstDecisionId));
            }
        });
    }

    [TestCase(LeaveStatus.Approved)]
    [TestCase(LeaveStatus.Rejected)]
    [TestCase(LeaveStatus.Cancelled)]
    public void Override_WhenAlreadyDecided_ReturnsFailedResultWithoutChangingRequest(
        LeaveStatus initialStatus
    )
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        if (initialStatus is LeaveStatus.Approved)
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: false,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
        }
        else if (initialStatus is LeaveStatus.Rejected)
        {
            var rejectResult = request.Reject(
                actorId: _actorReject,
                reason: _validRejectionReason,
                now: _thirdMarchAt
            );

            Assert.That(rejectResult.IsSuccess, Is.True);
        }
        else
        {
            var cancelResult = request.Cancel(
                actorId: request.EmployeeId,
                today: new DateOnly(2026, 3, 3),
                reason: _validCancellationReason,
                now: _thirdMarchAt
            );

            Assert.That(cancelResult.IsSuccess, Is.True);
        }

        Assert.That(request.Status, Is.EqualTo(initialStatus));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var originalDecisionId = request.Decisions[0].Id;

        // Act
        var result = request.Override(
            actorId: _overrideActorId,
            bypassedApproverId: _validBypassedId,
            outcome: DecisionType.Approve,
            reason: _validOverrideReason,
            now: _overrideAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request has already reached a final decision"));

            Assert.That(request.Status, Is.EqualTo(initialStatus));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
            Assert.That(request.Decisions[0].Id, Is.EqualTo(originalDecisionId));
        });
    }

    [Test]
    public void Override_ByRequesterInInvalidStatus_ReturnsSelfDecisionFailureWithoutChangingRequest()
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var approveFirstStageResult = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: false,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        Assert.That(approveFirstStageResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var originalDecisionId = request.Decisions[0].Id;

        // Act
        var result = request.Override(
            actorId: request.EmployeeId,
            bypassedApproverId: _validBypassedId,
            outcome: DecisionType.Approve,
            reason: _validOverrideReason,
            now: _fourthMarchAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Approver cannot decide their own leave request"));

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
            Assert.That(request.Decisions[0].Id, Is.EqualTo(originalDecisionId));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Override_InInvalidStatusWithInvalidContext_ReturnsFinalDecisionFailureWithoutChangingRequest(
        bool invalidReason
    )
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var approveFirstStageResult = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: false,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        Assert.That(approveFirstStageResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var originalDecisionId = request.Decisions[0].Id;
        var reason = invalidReason ? null : _validOverrideReason;
        DecisionType outcome = invalidReason ? DecisionType.Approve : DecisionType.Cancel;

        // Act
        var result = request.Override(
            actorId: _overrideActorId,
            bypassedApproverId: _validBypassedId,
            outcome: outcome,
            reason: reason,
            now: _fourthMarchAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Leave request has already reached a final decision"));

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));
        Assert.That(request.Decisions[0].Id, Is.EqualTo(originalDecisionId));
    }

    [TestCase(ApprovalStage.First, DecisionType.Approve)]
    [TestCase(ApprovalStage.First, DecisionType.Reject)]
    [TestCase(ApprovalStage.Final, DecisionType.Approve)]
    [TestCase(ApprovalStage.Final, DecisionType.Reject)]
    public void Override_WhenSuccessful_PreservesFrozenFieldsAndExistingHistory(
        ApprovalStage stage,
        DecisionType outcome
    )
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(stage);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        if (stage is ApprovalStage.Final)
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: true,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }

        var originalDecisionCount = request.Decisions.Count;

        var originalId = request.Id;
        var originalEmployeeId = request.EmployeeId;
        var originalPeriod = request.Period;
        var originalWorkingDays = request.WorkingDays;
        var originalAllocation = request.Allocation;
        var originalType = request.Type;
        var originalReason = request.Reason;
        var originalSubmittedAt = request.SubmittedAt;
        var originalSubmittedOffset = request.SubmittedAt.Offset;

        var originalDecisions = CaptureDecisionHistory(request);

        // Act
        var result = request.Override(
            actorId: _overrideActorId,
            bypassedApproverId: _validBypassedId,
            outcome: outcome,
            reason: _validOverrideReason,
            now: _overrideAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(request.Decisions, Has.Count.EqualTo(originalDecisionCount + 1));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.Id, Is.EqualTo(originalId));
            Assert.That(request.EmployeeId, Is.EqualTo(originalEmployeeId));
            Assert.That(request.Period, Is.EqualTo(originalPeriod));
            Assert.That(request.WorkingDays, Is.EqualTo(originalWorkingDays));
            Assert.That(request.Allocation, Is.EqualTo(originalAllocation));
            Assert.That(request.Type, Is.EqualTo(originalType));
            Assert.That(request.Reason, Is.EqualTo(originalReason));
            Assert.That(request.SubmittedAt, Is.EqualTo(originalSubmittedAt));
            Assert.That(request.SubmittedAt.Offset, Is.EqualTo(originalSubmittedOffset));

            for (var i = 0; i < originalDecisionCount; i++)
            {
                var originalDecision = originalDecisions[i];
                var currentDecision = request.Decisions[i];

                Assert.That(currentDecision.Id, Is.EqualTo(originalDecision.Id));
                Assert.That(currentDecision.LeaveRequestId, Is.EqualTo(originalDecision.LeaveRequestId));
                Assert.That(currentDecision.ActorId, Is.EqualTo(originalDecision.ActorId));
                Assert.That(currentDecision.Type, Is.EqualTo(originalDecision.Type));
                Assert.That(currentDecision.Stage, Is.EqualTo(originalDecision.Stage));
                Assert.That(currentDecision.Reason, Is.EqualTo(originalDecision.Reason));
                Assert.That(currentDecision.BypassedApproverId, Is.EqualTo(originalDecision.BypassedApproverId));
                Assert.That(currentDecision.OverrideOutcome, Is.EqualTo(originalDecision.OverrideOutcome));
                Assert.That(currentDecision.DecidedAt, Is.EqualTo(originalDecision.DecidedAt));
                Assert.That(currentDecision.DecidedAt.Offset, Is.EqualTo(originalDecision.DecidedAtOffset));
            }
        });
    }
}
