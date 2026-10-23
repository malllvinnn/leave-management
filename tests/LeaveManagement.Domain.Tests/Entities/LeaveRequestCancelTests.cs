using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class LeaveRequestCancelTests : LeaveRequestTestBase
{

    [TestCase("Pending", ApprovalStage.First)]
    [TestCase("PendingFinalApproval", ApprovalStage.Final)]
    [TestCase("ApprovedFirst", ApprovalStage.First)]
    [TestCase("ApprovedFinal", ApprovalStage.Final)]
    public void Cancel_BeforeStartDate_ReturnsCancelledRequestWithCorrectDecisionStage(
        string route,
        ApprovalStage expectedStage
    )
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(expectedStage);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));


        if (route == "PendingFinalApproval")
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: true,
                reason: _validReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }
        else if (route == "ApprovedFirst")
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: false,
                reason: _validReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }
        else if (route == "ApprovedFinal")
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: true,
                reason: _validReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));

            var approveFinalStageResult = request.ApproveFinalStage(
                approverId: _approverId,
                reason: _validApprovalFinalReason,
                now: _finalApprovalAt
            );

            Assert.That(approveFinalStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(request.Decisions, Has.Count.EqualTo(2));
        }

        var countBefore = request.Decisions.Count;

        // Act
        var result = request.Cancel(
            actorId: request.EmployeeId,
            today: new DateOnly(2026, 3, 15),
            reason: _validCancellationReason,
            now: _cancellationAt
        );

        // Pending
        Assert.That(result.IsSuccess, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Cancelled));
            Assert.That(request.Decisions, Has.Count.EqualTo(countBefore + 1));

            Assert.That(request.Decisions[countBefore].Type, Is.EqualTo(DecisionType.Cancel));
            Assert.That(request.Decisions[countBefore].Stage, Is.EqualTo(expectedStage));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Cancel_WithOptionalReason_PreservesDecisionReason(bool hasReason)
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var cancellationReason = hasReason ? _validCancellationReason : null;

        // Act
        var result = request.Cancel(
            actorId: request.EmployeeId,
            today: new DateOnly(2026, 3, 15),
            reason: cancellationReason,
            now: _cancellationAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.Decisions, Has.Count.EqualTo(1));
            Assert.That(request.Decisions[0].Reason, Is.EqualTo(cancellationReason));
        });
    }

    [TestCase(LeaveStatus.Pending, 0)]
    [TestCase(LeaveStatus.Pending, 1)]
    [TestCase(LeaveStatus.PendingFinalApproval, 0)]
    [TestCase(LeaveStatus.PendingFinalApproval, 1)]
    [TestCase(LeaveStatus.Approved, 0)]
    [TestCase(LeaveStatus.Approved, 1)]
    public void Cancel_OnOrAfterStartDate_ReturnsFailedResultWithoutChangingRequest(
        LeaveStatus initialStatus,
        int daysAfterStart
    )
    {
        // Arrange
        var stage = initialStatus switch
        {
            LeaveStatus.Pending or LeaveStatus.Approved => ApprovalStage.First,
            LeaveStatus.PendingFinalApproval => ApprovalStage.Final,
            _ => throw new ArgumentOutOfRangeException(nameof(initialStatus), initialStatus, null)
        };

        var createResult = CreatePendingRequestForStage(stage);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        if (initialStatus is LeaveStatus.PendingFinalApproval)
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
        else if (initialStatus is LeaveStatus.Approved)
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: false,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }

        Assert.That(request.Status, Is.EqualTo(initialStatus));

        Guid? originalDecisionId = request.Decisions.Count == 1
            ? request.Decisions[0].Id
            : null;

        var today = request.Period.Start.AddDays(daysAfterStart);

        var now = new DateTimeOffset(
            year: today.Year,
            month: today.Month,
            day: today.Day,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        // Act
        var result = request.Cancel(
            actorId: request.EmployeeId,
            today: today,
            reason: _validCancellationReason,
            now: now
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request cannot be cancelled on or after its start date"));

            Assert.That(request.Status, Is.EqualTo(initialStatus));
            Assert.That(request.Decisions, Has.Count.EqualTo(
                initialStatus is LeaveStatus.Pending ? 0 : 1
            ));

            if (originalDecisionId is not null)
            {
                Assert.That(request.Decisions[0].Id, Is.EqualTo(originalDecisionId));
            }
        });
    }

    [TestCase(LeaveStatus.Rejected)]
    [TestCase(LeaveStatus.Cancelled)]
    public void Cancel_WhenAlreadyRejectedOrCancelled_ReturnsFailedResultWithoutChangingRequest(
        LeaveStatus initialStatus
    )
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var stateAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 14,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        var stateToday = new DateOnly(2026, 3, 14);

        if (initialStatus is LeaveStatus.Rejected)
        {
            var rejectResult = request.Reject(
                actorId: _actorReject,
                reason: _validRejectionReason,
                now: stateAt
            );

            Assert.That(rejectResult.IsSuccess, Is.True);
        }
        else
        {
            var cancelResult = request.Cancel(
                actorId: request.EmployeeId,
                today: stateToday,
                reason: _validCancellationReason,
                now: stateAt
            );

            Assert.That(cancelResult.IsSuccess, Is.True);
        }

        Assert.That(request.Status, Is.EqualTo(initialStatus));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var originalDecisionId = request.Decisions[0].Id;

        // Act
        var result = request.Cancel(
            actorId: request.EmployeeId,
            today: new DateOnly(2026, 3, 15),
            reason: _validCancellationReason,
            now: _cancellationAt
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

    [TestCase(LeaveStatus.Rejected)]
    [TestCase(LeaveStatus.Cancelled)]
    public void Cancel_InInvalidStatusOnStartDate_ReturnsFinalDecisionFailureWithoutChangingRequest(
        LeaveStatus initialStatus
    )
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        if (initialStatus is LeaveStatus.Rejected)
        {
            var rejectResult = request.Reject(
                actorId: _approverId,
                reason: _validRejectionReason,
                now: _thirdMarchAt
            );

            Assert.That(rejectResult.IsSuccess, Is.True);
        }
        else
        {
            var cancelResult = request.Cancel(
                actorId: request.EmployeeId,
                today: _thirdMarchToday,
                reason: _validCancellationReason,
                now: _thirdMarchAt
            );

            Assert.That(cancelResult.IsSuccess, Is.True);
        }

        Assert.That(request.Status, Is.EqualTo(initialStatus));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var originalDecisionId = request.Decisions[0].Id;
        var sixtheenthMarchToday = new DateOnly(2026, 3, 16);

        var sixtheenthMarchAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 16,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        // Act
        var result = request.Cancel(
            actorId: request.EmployeeId,
            today: sixtheenthMarchToday,
            reason: _validCancellationReason,
            now: sixtheenthMarchAt
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
    public void Cancel_WithValidContext_PreservesDecisionContext()
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var cancelToday = new DateOnly(2026, 3, 15);

        // Act
        var result = request.Cancel(
            actorId: request.EmployeeId,
            today: cancelToday,
            reason: _validCancellationReason,
            now: _cancellationAt
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
            Assert.That(decision.ActorId, Is.EqualTo(request.EmployeeId));
            Assert.That(decision.Reason, Is.EqualTo(_validCancellationReason));
            Assert.That(decision.BypassedApproverId, Is.Null);
            Assert.That(decision.OverrideOutcome, Is.Null);
            Assert.That(decision.DecidedAt, Is.EqualTo(_cancellationAt.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

    [TestCase("Pending")]
    [TestCase("PendingFinalApproval")]
    [TestCase("ApprovedFirst")]
    [TestCase("ApprovedFinal")]
    public void Cancel_WhenSuccessful_PreservesFrozenFieldsAndExistingHistory(string route)
    {
        // Arrange
        var stage = route is "Pending" or "ApprovedFirst"
            ? ApprovalStage.First
            : ApprovalStage.Final;

        var createResult = CreatePendingRequestForStage(stage);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        if (route is "PendingFinalApproval")
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
        else if (route is "ApprovedFirst")
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: false,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
        }
        else if (route is "ApprovedFinal")
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

            var approveFinalStageResult = request.ApproveFinalStage(
                approverId: _approverId,
                reason: _validApprovalFinalReason,
                now: _finalApprovalAt
            );

            Assert.That(approveFinalStageResult.IsSuccess, Is.True);
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(request.Decisions, Has.Count.EqualTo(2));
        }

        var cancelToday = new DateOnly(2026, 3, 15);
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
        var result = request.Cancel(
            actorId: request.EmployeeId,
            today: cancelToday,
            reason: _validCancellationReason,
            now: _cancellationAt
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

    [TestCase(ApprovalStage.First)]
    [TestCase(ApprovalStage.Final)]
    public void Cancel_AfterOverrideApproval_PreservesOverrideOutcomeAndUsesItsStage(
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

        var overrideResult = request.Override(
            actorId: _overrideActorId,
            bypassedApproverId: _validBypassedId,
            outcome: DecisionType.Approve,
            reason: _validOverrideReason,
            now: _overrideAt
        );

        Assert.That(overrideResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));

        Assert.That(request.Decisions, Has.Count.EqualTo(
            stage is ApprovalStage.First ? 1 : 2
        ));

        var overrideDecisionIndex = stage is ApprovalStage.First ? 0 : 1;
        var originalOverrideDecisionId = request.Decisions[overrideDecisionIndex].Id;
        var countBefore = request.Decisions.Count;

        // Act
        var result = request.Cancel(
            actorId: request.EmployeeId,
            today: new DateOnly(2026, 3, 15),
            reason: _validCancellationReason,
            now: _cancellationAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Cancelled));
        Assert.That(request.Decisions, Has.Count.EqualTo(countBefore + 1));

        var cancelDecision = request.Decisions[countBefore];
        var originalOverrideDecision = request.Decisions[overrideDecisionIndex];

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(cancelDecision.Type, Is.EqualTo(DecisionType.Cancel));
            Assert.That(cancelDecision.Stage, Is.EqualTo(stage));

            Assert.That(originalOverrideDecision.Id, Is.EqualTo(originalOverrideDecisionId));
            Assert.That(originalOverrideDecision.Type, Is.EqualTo(DecisionType.Override));
            Assert.That(originalOverrideDecision.Stage, Is.EqualTo(stage));
            Assert.That(originalOverrideDecision.OverrideOutcome, Is.EqualTo(DecisionType.Approve));
        });
    }
}
