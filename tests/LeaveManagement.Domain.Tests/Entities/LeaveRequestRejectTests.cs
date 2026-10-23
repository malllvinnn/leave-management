using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class LeaveRequestRejectTests : LeaveRequestTestBase
{

    [TestCase(ApprovalStage.First)]
    [TestCase(ApprovalStage.Final)]
    public void Reject_WithValidReason_ReturnsRejectedRequestWithDecisionForCurrentStage(
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
            var approvalFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: true,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approvalFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }

        // Act
        var result = request.Reject(
            actorId: _actorReject,
            reason: _validRejectionReason,
            now: _rejectionAt
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

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Rejected));
            Assert.That(request.Decisions[decisionIndex].Type, Is.EqualTo(DecisionType.Reject));
            Assert.That(request.Decisions[decisionIndex].Stage, Is.EqualTo(stage));
            Assert.That(request.Decisions[decisionIndex].Reason, Is.EqualTo(_validRejectionReason));
        });
    }

    [TestCase(ApprovalStage.First)]
    [TestCase(ApprovalStage.Final)]
    public void Reject_WithNullReason_ReturnsFailedResultWithoutChangingRequest(
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
            var approvalFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: true,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approvalFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }

        Guid? firstDecisionId = stage is ApprovalStage.Final
            ? request.Decisions[0].Id
            : null;

        // Act
        var result = request.Reject(
            actorId: _actorReject,
            reason: null,
            now: _rejectionAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Rejection reason cannot be empty"));

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
    public void Reject_ByRequester_ReturnsFailedResultWithoutChangingRequest(
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
            var approvalFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: true,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approvalFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }

        Guid? firstDecisionId = stage is ApprovalStage.Final
            ? request.Decisions[0].Id
            : null;

        // Act
        var result = request.Reject(
            actorId: request.EmployeeId,
            reason: _validRejectionReason,
            now: _rejectionAt
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
    public void Reject_WhenAlreadyDecided_ReturnsFailedResultWithoutChangingRequest(LeaveStatus initialStatus)
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
        else
        {
            var rejectResult = request.Reject(
                actorId: _actorReject,
                reason: _validRejectionReason,
                now: new DateTimeOffset(
                    year: 2026,
                    month: 3,
                    day: 3,
                    hour: 10,
                    minute: 0,
                    second: 0,
                    offset: TimeSpan.FromHours(7)
                )
            );

            Assert.That(rejectResult.IsSuccess, Is.True);
        }

        Assert.That(request.Status, Is.EqualTo(initialStatus));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var originalDecisionId = request.Decisions[0].Id;

        // Act
        var result = request.Reject(
            actorId: _actorReject,
            reason: _validRejectionReason,
            now: _rejectionAt
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
    public void Reject_WhenCancelled_ReturnsFailedResultWithoutChangingRequest()
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var cancelResult = request.Cancel(
            actorId: request.EmployeeId,
            today: _thirdMarchToday,
            reason: _validCancellationReason,
            now: _thirdMarchAt
        );

        Assert.That(cancelResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Cancelled));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));
        Assert.That(request.Decisions[0].Type, Is.EqualTo(DecisionType.Cancel));

        var originalDecisionId = request.Decisions[0].Id;

        // Act
        var result = request.Reject(
            actorId: _approverId,
            reason: _validRejectionReason,
            now: _fourthMarchAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request has already reached a final decision"));

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Cancelled));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
            Assert.That(request.Decisions[0].Id, Is.EqualTo(originalDecisionId));
        });
    }

    [Test]
    public void Reject_ByRequesterInInvalidStatus_ReturnsSelfDecisionFailureWithoutChangingRequest()
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
        var result = request.Reject(
            actorId: request.EmployeeId,
            reason: _validRejectionReason,
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

    [Test]
    public void Reject_InInvalidStatusWithNullReason_ReturnsFinalDecisionFailureWithoutChangingRequest()
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
        var result = request.Reject(
            actorId: _approverId,
            reason: null,
            now: _fourthMarchAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request has already reached a final decision"));

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
            Assert.That(request.Decisions[0].Id, Is.EqualTo(originalDecisionId));
        });
    }

    [Test]
    public void Reject_WithValidContext_PreservesDecisionContext()
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        // Act
        var result = request.Reject(
            actorId: _actorReject,
            reason: _validRejectionReason,
            now: _rejectionAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var decision = request.Decisions[0];

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(decision.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(decision.LeaveRequestId, Is.EqualTo(request.Id));
            Assert.That(decision.ActorId, Is.EqualTo(_actorReject));
            Assert.That(decision.Reason, Is.EqualTo(_validRejectionReason));
            Assert.That(decision.BypassedApproverId, Is.Null);
            Assert.That(decision.OverrideOutcome, Is.Null);
            Assert.That(decision.DecidedAt, Is.EqualTo(_rejectionAt.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

    [TestCase(ApprovalStage.First)]
    [TestCase(ApprovalStage.Final)]
    public void Reject_WhenSuccessful_PreservesFrozenFieldsAndExistingHistory(
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

        var originalDecisionCount = request.Decisions.Count;

        var originalId = request.Id;
        var originalEmployeeId = request.EmployeeId;
        var originalPeriod = request.Period;
        var originalWorkingDays = request.WorkingDays;
        var originalAllocation = request.Allocation;
        var originalType = request.Type;
        var originalReason = request.Reason;
        var originalSubmittedAt = request.SubmittedAt;
        var originalSubmittedAtOffset = request.SubmittedAt.Offset;

        var originalDecisions = CaptureDecisionHistory(request);

        // Act
        var result = request.Reject(
            actorId: _actorReject,
            reason: _validRejectionReason,
            now: _rejectionAt
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
            Assert.That(request.SubmittedAt.Offset, Is.EqualTo(originalSubmittedAtOffset));

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
