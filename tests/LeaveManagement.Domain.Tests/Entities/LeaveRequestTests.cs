using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class LeaveRequestTests
{
    private EmployeeId _employeeId;
    private LeaveDays _fiveWorkingDays;
    private LeaveType _annualType;
    private Reason _validReason;
    private DateOnly _earlyMarchToday;
    private DateTimeOffset _submittedAtPlusSeven;

    [SetUp]
    public void Setup()
    {
        _employeeId = EmployeeId.New();
        _fiveWorkingDays = LeaveDays.Create(5).Value;
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

        var allocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(3).Value,
            carryOver: LeaveDays.Create(2).Value
        ).Value;

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
}