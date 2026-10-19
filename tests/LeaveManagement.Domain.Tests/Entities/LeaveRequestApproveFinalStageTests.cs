using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
[TestFixture]
public class LeaveRequestApproveFinalStageTests : LeaveRequestTestBase
{

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void ApproveFinalStage_WhenAwaitingFinalApproval_ReturnsApprovedRequestWithFinalStageDecision(
        bool hasReason,
        bool sameApprover
    )
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, 23)
        ).Value;

        var workingDays = LeaveDays.Create(6).Value;

        var allocation = LeaveAllocation.Create(
            annual: workingDays,
            carryOver: LeaveDays.Zero
        ).Value;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var approveFirstStageResult = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: true,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        Assert.That(approveFirstStageResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        Reason? finalReason = hasReason ? _validApprovalFinalReason : null;
        var finalApproverId = sameApprover ? _approverId : EmployeeId.New();

        // Act
        var result = request.ApproveFinalStage(
            approverId: finalApproverId,
            reason: finalReason,
            now: _finalApprovalAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(request.Decisions, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(request.Decisions[1].Type, Is.EqualTo(DecisionType.Approve));
            Assert.That(request.Decisions[1].Stage, Is.EqualTo(ApprovalStage.Final));
            Assert.That(request.Decisions[1].Reason, Is.EqualTo(finalReason));
        });
    }

    [Test]
    public void ApproveFinalStage_ByRequester_ReturnsFailedResultWithoutChangingRequest()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, 23)
        ).Value;

        var workingDays = LeaveDays.Create(6).Value;

        var allocation = LeaveAllocation.Create(
            annual: workingDays,
            carryOver: LeaveDays.Zero
        ).Value;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var approveFirstStageResult = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: true,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        Assert.That(approveFirstStageResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var firstDecisionId = request.Decisions[0].Id;

        // Act
        var result = request.ApproveFinalStage(
            approverId: request.EmployeeId,
            reason: _validApprovalFinalReason,
            now: _finalApprovalAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Approver cannot decide their own leave request"));

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));

            Assert.That(request.Decisions[0].Id, Is.EqualTo(firstDecisionId));
        });
    }

    [TestCase(LeaveStatus.Pending)]
    [TestCase(LeaveStatus.Approved)]
    public void ApproveFinalStage_WhenNotAwaitingFinalApproval_ReturnsFailedResultWithoutChangingRequest(
        LeaveStatus initialStatus
    )
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, 20)
        ).Value;

        var workingDays = LeaveDays.Create(5).Value;

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(5).Value,
            carryOver: LeaveDays.Zero
        ).Value;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

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

        Assert.That(request.Status, Is.EqualTo(initialStatus));
        Assert.That(request.Decisions, Has.Count.EqualTo(
            initialStatus is LeaveStatus.Pending ? 0 : 1
        ));

        Guid? firstDecisionId = initialStatus is LeaveStatus.Approved
            ? request.Decisions[0].Id
            : null;

        // Act
        var result = request.ApproveFinalStage(
            approverId: _approverId,
            reason: _validApprovalFinalReason,
            now: _finalApprovalAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request is not awaiting final approval"));

            Assert.That(request.Status, Is.EqualTo(initialStatus));

            Assert.That(request.Decisions, Has.Count.EqualTo(
                initialStatus is LeaveStatus.Pending ? 0 : 1
            ));

            if (initialStatus is LeaveStatus.Approved)
            {
                Assert.That(request.Decisions[0].Id, Is.EqualTo(firstDecisionId));
            }
        });
    }

    [Test]
    public void ApproveFinalStage_WhenAlreadyApprovedByFinalStage_ReturnsFailedResultWithoutChangingRequest()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, 23)
        ).Value;

        var workingDays = LeaveDays.Create(6).Value;

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(6).Value,
            carryOver: LeaveDays.Zero
        ).Value;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var approvalFirstStageResult = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: true,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        Assert.That(approvalFirstStageResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var approvalFinalStageResult = request.ApproveFinalStage(
            approverId: _approverId,
            reason: _validApprovalFinalReason,
            now: _finalApprovalAt
        );

        Assert.That(approvalFinalStageResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
        Assert.That(request.Decisions, Has.Count.EqualTo(2));

        var firstDecisionId = request.Decisions[0].Id;
        var finalDecisionId = request.Decisions[1].Id;

        var nowResult = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 4,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        // Act
        var result = request.ApproveFinalStage(
            approverId: _approverId,
            reason: _validApprovalFinalReason,
            now: nowResult
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request is not awaiting final approval"));

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(request.Decisions, Has.Count.EqualTo(2));

            Assert.That(request.Decisions[0].Id, Is.EqualTo(firstDecisionId));
            Assert.That(request.Decisions[1].Id, Is.EqualTo(finalDecisionId));
        });
    }

    [TestCase(LeaveStatus.Rejected)]
    [TestCase(LeaveStatus.Cancelled)]
    public void ApproveFinalStage_WhenRejectedOrCancelled_ReturnsFailedResultWithoutChangingRequest(
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
                today: _thirdMarchToday,
                reason: _validCancellationReason,
                now: _thirdMarchAt
            );

            Assert.That(cancelResult.IsSuccess, Is.True);
        }

        Assert.That(request.Status, Is.EqualTo(initialStatus));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        Assert.That(request.Decisions[0].Type, Is.EqualTo(
            initialStatus is LeaveStatus.Rejected
                ? DecisionType.Reject
                : DecisionType.Cancel
        ));

        var originalDecisionId = request.Decisions[0].Id;

        // Act
        var result = request.ApproveFinalStage(
            approverId: _approverId,
            reason: _validApprovalFinalReason,
            now: _fourthMarchAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request is not awaiting final approval"));

            Assert.That(request.Status, Is.EqualTo(initialStatus));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
            Assert.That(request.Decisions[0].Id, Is.EqualTo(originalDecisionId));
        });
    }

    [Test]
    public void ApproveFinalStage_ByRequesterInInvalidStatus_ReturnsSelfDecisionFailureWithoutChangingRequest()
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        // Act
        var result = request.ApproveFinalStage(
            approverId: request.EmployeeId,
            reason: _validApprovalFinalReason,
            now: _fourthMarchAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Approver cannot decide their own leave request"));

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
            Assert.That(request.Decisions, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public void ApproveFinalStage_WithValidContext_PreservesDecisionContext()
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.Final);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var approveFirstStageResult = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: true,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        Assert.That(approveFirstStageResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var finalActorId = EmployeeId.New();

        // Act
        var result = request.ApproveFinalStage(
            approverId: finalActorId,
            reason: _validApprovalFinalReason,
            now: _finalApprovalAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(request.Decisions, Has.Count.EqualTo(2));

        var decision = request.Decisions[1];

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(decision.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(decision.LeaveRequestId, Is.EqualTo(request.Id));
            Assert.That(decision.ActorId, Is.EqualTo(finalActorId));
            Assert.That(decision.Reason, Is.EqualTo(_validApprovalFinalReason));
            Assert.That(decision.BypassedApproverId, Is.Null);
            Assert.That(decision.OverrideOutcome, Is.Null);
            Assert.That(decision.DecidedAt, Is.EqualTo(_finalApprovalAt.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

    [Test]
    public void ApproveFinalStage_WhenSuccessful_PreservesFrozenFieldsAndExistingHistory()
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.Final);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var approveFirstStageResult = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: true,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        Assert.That(approveFirstStageResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var finalActorId = EmployeeId.New();
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
        var result = request.ApproveFinalStage(
            approverId: finalActorId,
            reason: _validApprovalFinalReason,
            now: _finalApprovalAt
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
