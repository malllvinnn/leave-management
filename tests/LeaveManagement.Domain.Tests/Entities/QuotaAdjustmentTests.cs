using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class QuotaAdjustmentTests
{
    private LeaveBalanceId _leaveBalanceId;
    private int _days;
    private Reason _reason;
    private EmployeeId _adjustedBy;
    private DateTimeOffset _adjustedAt;

    [SetUp]
    public void Setup()
    {
        _leaveBalanceId = LeaveBalanceId.New();
        _days = 5;
        _reason = Reason.Create("Annual leave quota correction").Value;
        _adjustedBy = EmployeeId.New();

        _adjustedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );
    }

    [Test]
    public void Create_WithValidValues_ReturnsSuccessfulResultWithCompleteState()
    {
        // Arrange
        var leaveBalanceId = LeaveBalanceId.New();
        int days = 5;
        var adjustedBy = EmployeeId.New();
        var reason = Reason.Create("Annual leave quota correction").Value;

        var adjustedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.Zero
        );

        // Act
        var result = QuotaAdjustment.Create(
            leaveBalanceId: leaveBalanceId,
            days: days,
            reason: reason,
            adjustedBy: adjustedBy,
            adjustedAt: adjustedAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Value.LeaveBalanceId, Is.EqualTo(leaveBalanceId));
            Assert.That(result.Value.Days, Is.EqualTo(days));
            Assert.That(result.Value.Reason, Is.EqualTo(reason));
            Assert.That(result.Value.AdjustedBy, Is.EqualTo(adjustedBy));
            Assert.That(result.Value.AdjustedAt, Is.EqualTo(adjustedAt));
        });
    }

    [Test]
    public void Create_WithZeroDays_ReturnsFailedResultWithError()
    {
        // Arrange
        int days = 0;

        // Act
        var result = QuotaAdjustment.Create(
            leaveBalanceId: _leaveBalanceId,
            days: days,
            reason: _reason,
            adjustedBy: _adjustedBy,
            adjustedAt: _adjustedAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Quota adjustment days cannot be zero"));
        });
    }

    [Test]
    public void Create_WithNullReason_ReturnsFailedResultWithError()
    {
        // Arrange
        Reason? reason = null;

        // Act
        var result = QuotaAdjustment.Create(
            leaveBalanceId: _leaveBalanceId,
            days: _days,
            reason: reason,
            adjustedBy: _adjustedBy,
            adjustedAt: _adjustedAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Quota adjustment reason cannot be empty"));
        });
    }

    [Test]
    public void Create_WithNonUtcTimestamp_ReturnsAdjustmentWithUtcTimestampPreservingInstant()
    {
        // Arrange
        var expectedAdjustedAtWithUtc = _adjustedAt.ToUniversalTime();

        // Act
        var result = QuotaAdjustment.Create(
            leaveBalanceId: _leaveBalanceId,
            days: _days,
            reason: _reason,
            adjustedBy: _adjustedBy,
            adjustedAt: _adjustedAt
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.AdjustedAt, Is.EqualTo(expectedAdjustedAtWithUtc));
            Assert.That(result.Value.AdjustedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }
}