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
}