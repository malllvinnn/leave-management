using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class LeaveRequestTests
{
    private EmployeeId _employeeId;
    private LeaveDays _fiveWorkingDays;
    private LeaveAllocation _threeAnnualPlusTwoCarryOverAllocation;
    private LeaveType _annualType;
    private Reason _validReason;
    private DateOnly _earlyMarchToday;
    private DateTimeOffset _submittedAtPlusSeven;
    private DateTimeOffset _submittedAtJustAfterNewYearUtcPlusSeven;
    private EmployeeId _approverId;
    private Reason _validApprovalReason;
    private DateTimeOffset _firstApprovalAt;
    private DateTimeOffset _finalApprovalAt;
    private Reason _validApprovalFinalReason;
    private EmployeeId _actorReject;
    private Reason _validRejectionReason;
    private DateTimeOffset _rejectionAt;
    private Reason _validCancellationReason;
    private DateTimeOffset _cancellationAt;
    private Reason _validOverrideReason;
    private EmployeeId _overrideActorId;
    private DateTimeOffset _overrideAt;
    private EmployeeId _validBypassedId;
    private DateTimeOffset _thirdMarchAt;
    private DateTimeOffset _fourthMarchAt;
    private DateOnly _thirdMarchToday;

    [SetUp]
    public void Setup()
    {
        _employeeId = EmployeeId.New();
        _fiveWorkingDays = LeaveDays.Create(5).Value;

        _threeAnnualPlusTwoCarryOverAllocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(3).Value,
            carryOver: LeaveDays.Create(2).Value
        ).Value;

        _annualType = LeaveType.AnnualLeave;
        _validReason = Reason.Create("valid reason").Value;
        _earlyMarchToday = new DateOnly(2026, 3, 1);

        _submittedAtPlusSeven = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 1,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _submittedAtJustAfterNewYearUtcPlusSeven = new DateTimeOffset(
            year: 2026,
            month: 1,
            day: 1,
            hour: 0,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _approverId = EmployeeId.New();
        _validApprovalReason = Reason.Create("Approved as scheduled").Value;

        _firstApprovalAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 2,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _finalApprovalAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 3,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _validApprovalFinalReason = Reason.Create("Final approval granted").Value;
        _actorReject = EmployeeId.New();
        _validRejectionReason = Reason.Create("Request cannot be approved").Value;

        _rejectionAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 4,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _validCancellationReason = Reason.Create("Plans have changed").Value;

        _cancellationAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 15,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _validOverrideReason = Reason.Create("Valid override reason").Value;
        _overrideActorId = EmployeeId.New();

        _overrideAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 4,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _validBypassedId = EmployeeId.New();

        _thirdMarchAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 3,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _fourthMarchAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 4,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _thirdMarchToday = new DateOnly(2026, 3, 3);
    }

    [Test]
    public void Create_WithValidValues_ReturnsPendingRequestWithFrozenValuesAndEmptyDecisions()
    {
        // Arrange
        var employeeId = _employeeId;

        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, 20)
        ).Value;

        var workingDays = _fiveWorkingDays;
        var allocation = _threeAnnualPlusTwoCarryOverAllocation;
        var type = _annualType;
        var reason = _validReason;
        var today = _earlyMarchToday;

        var submittedAt = _submittedAtPlusSeven;

        // Act
        var result = LeaveRequest.Create(
            employeeId: employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: type,
            reason: reason,
            today: today,
            submittedAt: submittedAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var request = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.Id.Value, Is.Not.EqualTo(Guid.Empty));
            Assert.That(request.EmployeeId, Is.EqualTo(employeeId));
            Assert.That(request.Period, Is.EqualTo(period));
            Assert.That(request.WorkingDays, Is.EqualTo(workingDays));
            Assert.That(request.Allocation, Is.EqualTo(allocation));
            Assert.That(request.Type, Is.EqualTo(type));
            Assert.That(request.Reason, Is.EqualTo(reason));

            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
            Assert.That(request.Decisions, Is.Empty);

            Assert.That(request.SubmittedAt, Is.EqualTo(submittedAt.ToUniversalTime()));
            Assert.That(request.SubmittedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

    [Test]
    public void Create_WithAllocationTotalDifferentFromWorkingDays_ReturnsFailure()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: new DateOnly(2026, 3, 20)
        ).Value;

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(2).Value,
            carryOver: LeaveDays.Create(2).Value
        ).Value;

        // Act
        var result = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: _fiveWorkingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Leave allocation must equal the working days of the request"));
    }

    [TestCase(LeaveType.AnnualLeave, 2025)]
    [TestCase(LeaveType.AnnualLeave, 2026)]
    [TestCase(LeaveType.SickLeave, 2025)]
    [TestCase(LeaveType.SickLeave, 2026)]
    public void Create_WithPeriodSpanningCalendarYears_ReturnsSingleCalendarYearFailure(
        LeaveType type,
        int startYear
    )
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(startYear, 12, 31),
            end: new DateOnly(startYear + 1, 1, 1)
        ).Value;

        var workingDays = LeaveDays.Create(2).Value;

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(2).Value,
            carryOver: LeaveDays.Create(0).Value
        ).Value;

        // Act
        var result = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: type,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request must be within a single calendar year"));
        });
    }

    [TestCase(LeaveType.AnnualLeave, 2025, 17)]
    [TestCase(LeaveType.AnnualLeave, 2027, 15)]
    [TestCase(LeaveType.SickLeave, 2025, 17)]
    [TestCase(LeaveType.SickLeave, 2027, 15)]
    public void Create_WithPeriodOutsideCurrentCalendarYear_ReturnsCurrentCalendarYearFailure(
        LeaveType type,
        int periodYear,
        int startDay
    )
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(periodYear, 3, startDay),
            end: new DateOnly(periodYear, 3, startDay + 4)
        ).Value;

        // Act
        var result = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: _fiveWorkingDays,
            allocation: _threeAnnualPlusTwoCarryOverAllocation,
            type: type,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request must be within the current calendar year"));
        });
    }

    [Test]
    public void Create_WithSickLeaveBackdatedIntoPreviousYear_ReturnsCurrentCalendarYearFailure()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2025, 12, 31),
            end: new DateOnly(2025, 12, 31)
        ).Value;

        var workingDays = LeaveDays.Create(1).Value;
        var type = LeaveType.SickLeave;

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(1).Value,
            carryOver: LeaveDays.Create(0).Value
        ).Value;

        var today = new DateOnly(2026, 1, 1);

        // Act
        var result = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: type,
            reason: _validReason,
            today: today,
            submittedAt: _submittedAtJustAfterNewYearUtcPlusSeven
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request must be within the current calendar year"));
        });
    }

    [TestCase(LeaveType.AnnualLeave, 1, 1)]
    [TestCase(LeaveType.AnnualLeave, 12, 31)]
    [TestCase(LeaveType.SickLeave, 1, 1)]
    [TestCase(LeaveType.SickLeave, 12, 31)]
    public void Create_WithPeriodWithinTodayCalendarYear_ReturnsSuccessfulRequest(
        LeaveType type,
        int month,
        int day
    )
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, month, day),
            end: new DateOnly(2026, month, day)
        ).Value;

        var workingDays = LeaveDays.Create(1).Value;

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(1).Value,
            carryOver: LeaveDays.Create(0).Value
        ).Value;

        var today = new DateOnly(2026, 1, 1);
        var submittedAt = _submittedAtJustAfterNewYearUtcPlusSeven;

        // Act
        var result = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: type,
            reason: _validReason,
            today: today,
            submittedAt: submittedAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var request = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.EmployeeId, Is.EqualTo(_employeeId));
            Assert.That(request.Period, Is.EqualTo(period));
            Assert.That(request.WorkingDays, Is.EqualTo(workingDays));
            Assert.That(request.Allocation, Is.EqualTo(allocation));
            Assert.That(request.Type, Is.EqualTo(type));
            Assert.That(request.Reason, Is.EqualTo(_validReason));
            Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));

            Assert.That(request.SubmittedAt, Is.EqualTo(submittedAt.ToUniversalTime()));
            Assert.That(request.SubmittedAt.Offset, Is.EqualTo(TimeSpan.Zero));

            Assert.That(request.Decisions, Is.Empty);
        });
    }

    [Test]
    public void Create_WithCrossYearPeriodAndAllocationMismatch_ReturnsSingleCalendarYearFailure()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2025, 12, 31),
            end: new DateOnly(2026, 1, 1)
        ).Value;

        var workingDays = LeaveDays.Create(2).Value;

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(1).Value,
            carryOver: LeaveDays.Create(0).Value
        ).Value;

        var today = new DateOnly(2026, 3, 1);

        // Act
        var result = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: today,
            submittedAt: _submittedAtPlusSeven
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request must be within a single calendar year"));
        });
    }

    [Test]
    public void Create_WithNonCurrentYearPeriodAndAllocationMismatch_ReturnsCurrentCalendarYearFailure()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2027, 3, 15),
            end: new DateOnly(2027, 3, 19)
        ).Value;

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(2).Value,
            carryOver: LeaveDays.Create(2).Value
        ).Value;

        var today = new DateOnly(2026, 3, 1);

        // Act
        var result = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: _fiveWorkingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: today,
            submittedAt: _submittedAtPlusSeven
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave request must be within the current calendar year"));
        });
    }

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
            Assert.That(request.Decisions, Is.Empty);
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
        Assert.That(request.Decisions, Is.Empty);

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
            Assert.That(request.Decisions, Is.Empty);
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
        Assert.That(request.Decisions, Is.Empty);

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

        if (stage == ApprovalStage.Final)
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
            stage == ApprovalStage.First ? 1 : 2
        ));

        var decisionIndex = stage == ApprovalStage.First ? 0 : 1;

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

        if (stage == ApprovalStage.Final)
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

        Guid? firstDecisionId = stage == ApprovalStage.Final
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
                stage == ApprovalStage.First
                    ? LeaveStatus.Pending
                    : LeaveStatus.PendingFinalApproval
            ));

            Assert.That(request.Decisions, Has.Count.EqualTo(
                stage == ApprovalStage.First ? 0 : 1
            ));

            if (stage == ApprovalStage.Final)
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

        if (stage == ApprovalStage.Final)
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

        Guid? firstDecisionId = stage == ApprovalStage.Final
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
                stage == ApprovalStage.First
                    ? LeaveStatus.Pending
                    : LeaveStatus.PendingFinalApproval
            ));

            Assert.That(request.Decisions, Has.Count.EqualTo(
                stage == ApprovalStage.First ? 0 : 1
            ));

            if (stage == ApprovalStage.Final)
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

        if (initialStatus == LeaveStatus.Approved)
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

        if (initialStatus == LeaveStatus.PendingFinalApproval)
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
        else if (initialStatus == LeaveStatus.Approved)
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
                initialStatus == LeaveStatus.Pending ? 0 : 1
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

        if (initialStatus == LeaveStatus.Rejected)
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

        if (stage == ApprovalStage.Final)
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
            stage == ApprovalStage.First ? 1 : 2
        ));

        var decisionIndex = stage == ApprovalStage.First ? 0 : 1;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(request.Status, Is.EqualTo(
                outcome == DecisionType.Approve ? LeaveStatus.Approved : LeaveStatus.Rejected
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

        if (stage == ApprovalStage.Final)
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

        Guid? firstDecisionId = stage == ApprovalStage.Final ? request.Decisions[0].Id : null;

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
                stage == ApprovalStage.First
                    ? LeaveStatus.Pending
                    : LeaveStatus.PendingFinalApproval
            ));

            Assert.That(request.Decisions, Has.Count.EqualTo(
                stage == ApprovalStage.First ? 0 : 1
            ));

            if (stage == ApprovalStage.Final)
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

        if (stage == ApprovalStage.Final)
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

        Guid? firstDecisionId = stage == ApprovalStage.Final ? request.Decisions[0].Id : null;

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
                stage == ApprovalStage.First
                    ? LeaveStatus.Pending
                    : LeaveStatus.PendingFinalApproval
            ));

            Assert.That(request.Decisions, Has.Count.EqualTo(
                stage == ApprovalStage.First ? 0 : 1
            ));

            if (stage == ApprovalStage.Final)
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

        if (stage == ApprovalStage.Final)
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

        Guid? firstDecisionId = stage == ApprovalStage.Final ? request.Decisions[0].Id : null;

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
                stage == ApprovalStage.First
                    ? LeaveStatus.Pending
                    : LeaveStatus.PendingFinalApproval
            ));

            Assert.That(request.Decisions, Has.Count.EqualTo(
                stage == ApprovalStage.First ? 0 : 1
            ));

            if (stage == ApprovalStage.Final)
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

        if (initialStatus == LeaveStatus.Approved)
        {
            var approveFirstStageResult = request.ApproveFirstStage(
                approverId: _approverId,
                requiresFinalApproval: false,
                reason: _validApprovalReason,
                now: _firstApprovalAt
            );

            Assert.That(approveFirstStageResult.IsSuccess, Is.True);
        }
        else if (initialStatus == LeaveStatus.Rejected)
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

        if (initialStatus == LeaveStatus.Rejected)
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
            initialStatus == LeaveStatus.Rejected
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

        if (initialStatus == LeaveStatus.Rejected)
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
            initialStatus == LeaveStatus.Rejected
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

        if (initialStatus == LeaveStatus.Rejected)
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
            Assert.That(decision.BypassedApproverId, Is.EqualTo(null));
            Assert.That(decision.OverrideOutcome, Is.EqualTo(null));
            Assert.That(decision.DecidedAt, Is.EqualTo(_firstApprovalAt.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
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
            Assert.That(decision.BypassedApproverId, Is.EqualTo(null));
            Assert.That(decision.OverrideOutcome, Is.EqualTo(null));
            Assert.That(decision.DecidedAt, Is.EqualTo(_finalApprovalAt.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
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
            Assert.That(decision.BypassedApproverId, Is.EqualTo(null));
            Assert.That(decision.OverrideOutcome, Is.EqualTo(null));
            Assert.That(decision.DecidedAt, Is.EqualTo(_rejectionAt.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
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
            Assert.That(decision.BypassedApproverId, Is.EqualTo(null));
            Assert.That(decision.OverrideOutcome, Is.EqualTo(null));
            Assert.That(decision.DecidedAt, Is.EqualTo(_cancellationAt.ToUniversalTime()));
            Assert.That(decision.DecidedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }

    private Result<LeaveRequest> CreatePendingRequestForStage(ApprovalStage stage)
    {
        var endPeriod = stage == ApprovalStage.First
            ? new DateOnly(2026, 3, 20)
            : new DateOnly(2026, 3, 23);

        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: endPeriod
        ).Value;

        var workingDays = stage == ApprovalStage.First
            ? LeaveDays.Create(5).Value
            : LeaveDays.Create(6).Value;

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

        return createResult;
    }
}
