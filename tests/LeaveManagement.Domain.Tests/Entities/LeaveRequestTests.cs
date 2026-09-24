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
}
