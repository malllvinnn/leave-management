using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
[TestFixture]
public class LeaveRequestApproveFirstStageTests : LeaveRequestTestBase
{

    [TestCase(1)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(10)]
    public void ApproveFirstStage_WithSingleStageRoute_ReturnsApprovedRequestWithFirstStageDecision(int workingDays)
    {
        // Arrange
        var endDay = workingDays switch
        {
            1 => 16,
            5 => 20,
            6 => 23,
            10 => 27,
            _ => throw new ArgumentOutOfRangeException(nameof(workingDays))
        };

        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, endDay)
        ).Value;

        var requestedWorkingDays = LeaveDays.Create(workingDays).Value;

        var allocation = LeaveAllocation.Create(
            annual: requestedWorkingDays,
            carryOver: LeaveDays.Zero
        ).Value;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: requestedWorkingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        // Act
        var result = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: false,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var decision = request.Decisions.Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(decision.Type, Is.EqualTo(DecisionType.Approve));
            Assert.That(decision.Stage, Is.EqualTo(ApprovalStage.First));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ApproveFirstStage_WithOptionalReason_PreservesDecisionReason(bool hasReason)
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, 20)
        ).Value;

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(5).Value,
            carryOver: LeaveDays.Zero
        ).Value;

        Reason? approvalReason = hasReason ? _validApprovalReason : null;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: _fiveWorkingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        // Act
        var result = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: false,
            reason: approvalReason,
            now: _firstApprovalAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var decision = request.Decisions.Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(decision.Reason, Is.EqualTo(approvalReason));
        });
    }

    [TestCase(6)]
    [TestCase(10)]
    public void ApproveFirstStage_WithFinalApprovalRequiredForLongRequest_ReturnsPendingFinalApprovalWithFirstStageDecision(int workingDays)
    {
        // Arrange
        var endDay = workingDays switch
        {
            6 => 23,
            10 => 27,
            _ => throw new ArgumentOutOfRangeException(nameof(workingDays))
        };

        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, endDay)
        ).Value;

        var requestedWorkingDays = LeaveDays.Create(workingDays).Value;

        var allocation = LeaveAllocation.Create(
            annual: requestedWorkingDays,
            carryOver: LeaveDays.Zero
        ).Value;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: requestedWorkingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        // Act
        var result = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: true,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var decision = request.Decisions.Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.PendingFinalApproval));
            Assert.That(decision.Type, Is.EqualTo(DecisionType.Approve));
            Assert.That(decision.Stage, Is.EqualTo(ApprovalStage.First));
        });
    }

    [TestCase(1)]
    [TestCase(5)]
    public void ApproveFirstStage_WithFinalApprovalRequiredForShortRequest_ReturnsFailedResultWithoutChangingRequest(int workingDays)
    {
        // Arrange
        var endDay = workingDays switch
        {
            1 => 16,
            5 => 20,
            _ => throw new ArgumentOutOfRangeException(nameof(workingDays))
        };

        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, endDay)
        ).Value;

        var requestedWorkingDays = LeaveDays.Create(workingDays).Value;

        var allocation = LeaveAllocation.Create(
            annual: requestedWorkingDays,
            carryOver: LeaveDays.Zero
        ).Value;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: requestedWorkingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        var originalId = request.Id;
        var originalEmployeeId = request.EmployeeId;
        var originalPeriod = request.Period;
        var originalWorkingDays = request.WorkingDays;
        var originalAllocation = request.Allocation;
        var originalType = request.Type;
        var originalReason = request.Reason;
        var originalSubmittedAt = request.SubmittedAt;
        var originalSubmittedAtOffset = request.SubmittedAt.Offset;

        // Act
        var result = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: true,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Final approval cannot be required for requests of 5 working days or fewer"));

            Assert.That(request.Id, Is.EqualTo(originalId));
            Assert.That(request.EmployeeId, Is.EqualTo(originalEmployeeId));
            Assert.That(request.Period, Is.EqualTo(originalPeriod));
            Assert.That(request.WorkingDays, Is.EqualTo(originalWorkingDays));
            Assert.That(request.Allocation, Is.EqualTo(originalAllocation));
            Assert.That(request.Type, Is.EqualTo(originalType));
            Assert.That(request.Reason, Is.EqualTo(originalReason));
            Assert.That(request.SubmittedAt, Is.EqualTo(originalSubmittedAt));
            Assert.That(request.SubmittedAt.Offset, Is.EqualTo(originalSubmittedAtOffset));

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
            Assert.That(request.Decisions, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public void ApproveFirstStage_ByRequester_ReturnsFailedResultWithoutChangingRequest()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, 20)
        ).Value;

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(5).Value,
            carryOver: LeaveDays.Zero
        ).Value;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: _fiveWorkingDays,
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

        var originalId = request.Id;
        var originalEmployeeId = request.EmployeeId;
        var originalPeriod = request.Period;
        var originalWorkingDays = request.WorkingDays;
        var originalAllocation = request.Allocation;
        var originalType = request.Type;
        var originalReason = request.Reason;
        var originalStatus = request.Status;
        var originalSubmittedAt = request.SubmittedAt;
        var originalSubmittedAtOffset = request.SubmittedAt.Offset;

        // Act
        var result = request.ApproveFirstStage(
            approverId: request.EmployeeId,
            requiresFinalApproval: false,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Approver cannot decide their own leave request"));

            Assert.That(request.Id, Is.EqualTo(originalId));
            Assert.That(request.EmployeeId, Is.EqualTo(originalEmployeeId));
            Assert.That(request.Period, Is.EqualTo(originalPeriod));
            Assert.That(request.WorkingDays, Is.EqualTo(originalWorkingDays));
            Assert.That(request.Allocation, Is.EqualTo(originalAllocation));
            Assert.That(request.Type, Is.EqualTo(originalType));
            Assert.That(request.Reason, Is.EqualTo(originalReason));
            Assert.That(request.SubmittedAt, Is.EqualTo(originalSubmittedAt));
            Assert.That(request.SubmittedAt.Offset, Is.EqualTo(originalSubmittedAtOffset));

            Assert.That(request.Status, Is.EqualTo(originalStatus));
            Assert.That(request.Decisions, Has.Count.EqualTo(0));
        });
    }

    [TestCase(6, true, LeaveStatus.PendingFinalApproval)]
    [TestCase(5, false, LeaveStatus.Approved)]
    public void ApproveFirstStage_WhenNotPending_ReturnsFailedResultWithoutChangingRequest(
        int workingDays,
        bool firstRequiresFinalApproval,
        LeaveStatus expectedStatus
    )
    {
        // Arrange
        var endDay = workingDays switch
        {
            6 => 23,
            5 => 20,
            _ => throw new ArgumentOutOfRangeException(nameof(workingDays))
        };

        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, endDay)
        ).Value;

        var requestedWorkingDays = LeaveDays.Create(workingDays).Value;

        var allocation = LeaveAllocation.Create(
            annual: requestedWorkingDays,
            carryOver: LeaveDays.Zero
        ).Value;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: requestedWorkingDays,
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
            requiresFinalApproval: firstRequiresFinalApproval,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        Assert.That(approveFirstStageResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(expectedStatus));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var originalId = request.Id;
        var originalEmployeeId = request.EmployeeId;
        var originalPeriod = request.Period;
        var originalWorkingDays = request.WorkingDays;
        var originalAllocation = request.Allocation;
        var originalType = request.Type;
        var originalReason = request.Reason;
        var originalStatus = request.Status;
        var originalSubmittedAt = request.SubmittedAt;
        var originalSubmittedAtOffset = request.SubmittedAt.Offset;

        var firstDecision = request.Decisions[0];
        var originalDecisionId = firstDecision.Id;
        var originalDecisionRequestId = firstDecision.LeaveRequestId;
        var originalDecisionActorId = firstDecision.ActorId;
        var originalDecisionType = firstDecision.Type;
        var originalDecisionStage = firstDecision.Stage;
        var originalDecisionReason = firstDecision.Reason;
        var originalDecisionBypassedApproverId = firstDecision.BypassedApproverId;
        var originalDecisionOverrideOutcome = firstDecision.OverrideOutcome;
        var originalDecisionDecidedAt = firstDecision.DecidedAt;
        var originalDecisionDecidedAtOffset = firstDecision.DecidedAt.Offset;

        var approvalAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 3,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        // Act
        var result = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: false,
            reason: _validApprovalReason,
            now: approvalAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request is not awaiting first stage approval"));

            Assert.That(request.Status, Is.EqualTo(expectedStatus));

            Assert.That(request.Id, Is.EqualTo(originalId));
            Assert.That(request.EmployeeId, Is.EqualTo(originalEmployeeId));
            Assert.That(request.Period, Is.EqualTo(originalPeriod));
            Assert.That(request.WorkingDays, Is.EqualTo(originalWorkingDays));
            Assert.That(request.Allocation, Is.EqualTo(originalAllocation));
            Assert.That(request.Type, Is.EqualTo(originalType));
            Assert.That(request.Reason, Is.EqualTo(originalReason));
            Assert.That(request.Status, Is.EqualTo(originalStatus));
            Assert.That(request.SubmittedAt, Is.EqualTo(originalSubmittedAt));
            Assert.That(request.SubmittedAt.Offset, Is.EqualTo(originalSubmittedAtOffset));

            Assert.That(request.Decisions, Has.Count.EqualTo(1));

            Assert.That(request.Decisions[0].Id, Is.EqualTo(originalDecisionId));
            Assert.That(request.Decisions[0].LeaveRequestId, Is.EqualTo(originalDecisionRequestId));
            Assert.That(request.Decisions[0].ActorId, Is.EqualTo(originalDecisionActorId));
            Assert.That(request.Decisions[0].Type, Is.EqualTo(originalDecisionType));
            Assert.That(request.Decisions[0].Stage, Is.EqualTo(originalDecisionStage));
            Assert.That(request.Decisions[0].Reason, Is.EqualTo(originalDecisionReason));
            Assert.That(request.Decisions[0].BypassedApproverId, Is.EqualTo(originalDecisionBypassedApproverId));
            Assert.That(request.Decisions[0].OverrideOutcome, Is.EqualTo(originalDecisionOverrideOutcome));
            Assert.That(request.Decisions[0].DecidedAt, Is.EqualTo(originalDecisionDecidedAt));
            Assert.That(request.Decisions[0].DecidedAt.Offset, Is.EqualTo(originalDecisionDecidedAtOffset));
        });
    }

    [TestCase(LeaveStatus.Rejected)]
    [TestCase(LeaveStatus.Cancelled)]
    public void ApproveFirstStage_WhenRejectedOrCancelled_ReturnsFailedResultWithoutChangingRequest(
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
        var result = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: false,
            reason: _validApprovalReason,
            now: _fourthMarchAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request is not awaiting first stage approval"));

            Assert.That(request.Status, Is.EqualTo(initialStatus));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
            Assert.That(request.Decisions[0].Id, Is.EqualTo(originalDecisionId));
        });
    }

    [Test]
    public void ApproveFirstStage_ByRequesterInInvalidStatus_ReturnsSelfDecisionFailureWithoutChangingRequest()
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
        var result = request.ApproveFirstStage(
            approverId: request.EmployeeId,
            requiresFinalApproval: false,
            reason: _validApprovalReason,
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
    public void ApproveFirstStage_InInvalidStatusWithInvalidRoute_ReturnsStageFailureWithoutChangingRequest()
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
        var result = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: true,
            reason: _validApprovalReason,
            now: _fourthMarchAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request is not awaiting first stage approval"));

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
            Assert.That(request.Decisions, Has.Count.EqualTo(1));
            Assert.That(request.Decisions[0].Id, Is.EqualTo(originalDecisionId));
        });
    }

    [Test]
    public void ApproveFirstStage_WithValidContext_PreservesDecisionContext()
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        // Act
        var result = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: false,
            reason: _validApprovalReason,
            now: _firstApprovalAt
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
            Assert.That(decision.ActorId, Is.EqualTo(_approverId));
            Assert.That(decision.Reason, Is.EqualTo(_validApprovalReason));
            Assert.That(decision.BypassedApproverId, Is.Null);
            Assert.That(decision.OverrideOutcome, Is.Null);
            Assert.That(decision.DecidedAt, Is.EqualTo(_firstApprovalAt.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ApproveFirstStage_WhenSuccessful_PreservesFrozenFieldsAndExistingHistory(
        bool requiresFinalApproval
    )
    {
        // Arrange
        var stage = requiresFinalApproval ? ApprovalStage.Final : ApprovalStage.First;
        var createResult = CreatePendingRequestForStage(stage);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

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

        // Act
        var result = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: requiresFinalApproval,
            reason: _validApprovalReason,
            now: _firstApprovalAt
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
        });
    }
}
