using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class LeaveBalanceAdjustTests : LeaveBalanceTestBase
{

    [Test]
    public void Adjust_WithPositiveDays_ReturnsSuccessfulResultAndIncreasesAnnualQuota()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        int days = 5;

        var originalAnnualQuota = leaveBalance.AnnualQuota;
        var expectedAnnualQuotaValue = originalAnnualQuota.Value + days;

        // Act
        var result = leaveBalance.Adjust(
            days: days,
            reason: _reasonAdjustment,
            actorId: _employeeId,
            now: _dateTimeNow
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.AnnualQuota.Value, Is.EqualTo(expectedAnnualQuotaValue));
        });
    }

    [Test]
    public void Adjust_WithNegativeDays_ReturnsSuccessfulResultAndDecreasesAnnualQuota()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        int days = -3;

        var originalAnnualQuota = leaveBalance.AnnualQuota;
        var expectedAnnualQuotaValue = originalAnnualQuota.Value + days;

        // Act
        var result = leaveBalance.Adjust(
            days: days,
            reason: _reasonAdjustment,
            actorId: _employeeId,
            now: _dateTimeNow
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.AnnualQuota.Value, Is.EqualTo(expectedAnnualQuotaValue));
        });
    }

    [Test]
    public void Adjust_WithZeroDays_ReturnsFailedResultWithError()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        int days = 0;

        // Act
        var result = leaveBalance.Adjust(
            days: days,
            reason: _reasonAdjustment,
            actorId: _employeeId,
            now: _dateTimeNow
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
    public void Adjust_WithNullReason_ReturnsFailedResultWithError()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var days = 5;
        Reason? reason = null;

        // Act
        var result = leaveBalance.Adjust(
            days: days,
            reason: reason,
            actorId: _employeeId,
            now: _dateTimeNow
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
    public void Adjust_MakingAvailableNegative_ReturnsFailedResultAndDoesNotRecordAdjustment()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var afterExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(1);

        // annual used 5
        var usedDays = LeaveDays.Create(5).Value;

        var reserveForUsageResult = leaveBalance.Reserve(
            days: usedDays,
            asOf: afterExpiryDate
        );

        Assert.That(reserveForUsageResult.IsSuccess, Is.True);

        var confirmUsageResult = leaveBalance.ConfirmUsage(
            allocation: reserveForUsageResult.Value
        );

        Assert.That(confirmUsageResult.IsSuccess, Is.True);

        // annual reserved 4
        var reserveDays = LeaveDays.Create(4).Value;

        var reserveResult = leaveBalance.Reserve(
            days: reserveDays,
            asOf: afterExpiryDate
        );

        Assert.That(reserveResult.IsSuccess, Is.True);

        // annual quota 12 becomes 8, below annual activity floor 9
        int adjustmentDays = -4;

        var originalAnnualQuota = leaveBalance.AnnualQuota;
        var originalCarriedOver = leaveBalance.CarriedOver;
        var originalAnnualUsed = leaveBalance.AnnualUsed;
        var originalAnnualReserved = leaveBalance.AnnualReserved;
        var originalCarryOverUsed = leaveBalance.CarryOverUsed;
        var originalCarryOverReserved = leaveBalance.CarryOverReserved;

        var originalAdjustmentCount = leaveBalance.Adjustments.Count;

        // Act
        var result = leaveBalance.Adjust(
            days: adjustmentDays,
            reason: _reasonAdjustment,
            actorId: _employeeId,
            now: _dateTimeNow
        );

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Quota adjustment cannot make the available balance negative"));

        Assert.That(leaveBalance.AnnualQuota, Is.EqualTo(originalAnnualQuota));
        Assert.That(leaveBalance.CarriedOver, Is.EqualTo(originalCarriedOver));
        Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(originalAnnualUsed));
        Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(originalAnnualReserved));
        Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(originalCarryOverUsed));
        Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(originalCarryOverReserved));
        Assert.That(leaveBalance.Adjustments.Count, Is.EqualTo(originalAdjustmentCount));
    }

    [Test]
    public void Adjust_CalledMultipleTimes_AppendsEveryAdjustmentInOrder()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        int firstAdjustmentDays = 5;
        int secondAdjustmentDays = -2;

        var originalAdjustmentCount = leaveBalance.Adjustments.Count;

        // Act
        var firstResult = leaveBalance.Adjust(
            days: firstAdjustmentDays,
            reason: _reasonAdjustment,
            actorId: _employeeId,
            now: _dateTimeNow
        );

        var secondResult = leaveBalance.Adjust(
            days: secondAdjustmentDays,
            reason: _reasonAdjustment,
            actorId: _employeeId,
            now: _dateTimeNow
        );

        var firstRecordAdjustment = leaveBalance.Adjustments[originalAdjustmentCount]; // index 0
        var secondRecordAdjustment = leaveBalance.Adjustments[originalAdjustmentCount + 1]; // index 1

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(firstResult.IsSuccess, Is.True);
            Assert.That(secondResult.IsSuccess, Is.True);

            Assert.That(leaveBalance.Adjustments, Has.Count.EqualTo(originalAdjustmentCount + 2));

            Assert.That(firstRecordAdjustment.Days, Is.EqualTo(firstAdjustmentDays));
            Assert.That(secondRecordAdjustment.Days, Is.EqualTo(secondAdjustmentDays));
        });
    }

    [Test]
    public void Adjust_WithValidValues_DoesNotChangeCarriedOverOrBuckets()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);
        var reserveDays = LeaveDays.Create(6).Value;

        var reserveResult = leaveBalance.Reserve(
            days: reserveDays,
            asOf: beforeExpiryDate
        );

        Assert.That(reserveResult.IsSuccess, Is.True);

        var originalAnnualQuota = leaveBalance.AnnualQuota;
        var originalCarriedOver = leaveBalance.CarriedOver;
        var originalAnnualUsed = leaveBalance.AnnualUsed;
        var originalAnnualReserved = leaveBalance.AnnualReserved;
        var originalCarryOverUsed = leaveBalance.CarryOverUsed;
        var originalCarryOverReserved = leaveBalance.CarryOverReserved;

        var adjustmentDays = 5;
        var expectedAnnualQuotaValue = originalAnnualQuota.Value + adjustmentDays;

        // Act
        var result = leaveBalance.Adjust(
            days: adjustmentDays,
            reason: _reasonAdjustment,
            actorId: _employeeId,
            now: _dateTimeNow
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.AnnualQuota.Value, Is.EqualTo(expectedAnnualQuotaValue));
            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(originalCarriedOver));
            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(originalAnnualUsed));
            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(originalAnnualReserved));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(originalCarryOverUsed));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(originalCarryOverReserved));
        });
    }

    [Test]
    public void Adjust_WithPositiveDays_RecordsCompleteAdjustment()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var originalAdjustmentCount = leaveBalance.Adjustments.Count;
        var adjustmentDays = 5;

        // Act
        var result = leaveBalance.Adjust(
            days: adjustmentDays,
            reason: _reasonAdjustment,
            actorId: _employeeId,
            now: _dateTimeNow
        );

        var recordedAdjustment = leaveBalance.Adjustments.Last();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.Adjustments, Has.Count.EqualTo(originalAdjustmentCount + 1));
            Assert.That(recordedAdjustment.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(recordedAdjustment.LeaveBalanceId, Is.EqualTo(leaveBalance.Id));
            Assert.That(recordedAdjustment.Days, Is.EqualTo(adjustmentDays));
            Assert.That(recordedAdjustment.Reason, Is.EqualTo(_reasonAdjustment));
            Assert.That(recordedAdjustment.AdjustedBy, Is.EqualTo(_employeeId));
            Assert.That(recordedAdjustment.AdjustedAt, Is.EqualTo(_dateTimeNow));
        });
    }

    [Test]
    public void Adjust_WithNegativeDays_PreservesSignedDaysInAdjustment()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        int adjustmentDays = -3;
        var originalAdjustmentCount = leaveBalance.Adjustments.Count;

        // Act
        var result = leaveBalance.Adjust(
            days: adjustmentDays,
            reason: _reasonAdjustment,
            actorId: _employeeId,
            now: _dateTimeNow
        );

        var recordedAdjustment = leaveBalance.Adjustments.Last();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.Adjustments, Has.Count.EqualTo(originalAdjustmentCount + 1));
            Assert.That(recordedAdjustment.Days, Is.EqualTo(adjustmentDays));
        });
    }

    [Test]
    public void Adjust_WithNonUtcTimestamp_StoresTimestampInUtcPreservingInstant()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var adjustmentDays = 5;

        var dateTimeNowNonUtc = new DateTimeOffset(
            year: _year,
            month: 9,
            day: 8,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        var expectedUtcTimestamp = dateTimeNowNonUtc.ToUniversalTime();
        var originalAdjustmentCount = leaveBalance.Adjustments.Count;

        // Act
        var result = leaveBalance.Adjust(
            days: adjustmentDays,
            reason: _reasonAdjustment,
            actorId: _employeeId,
            now: dateTimeNowNonUtc
        );

        var recordedAdjustment = leaveBalance.Adjustments.Last();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.Adjustments, Has.Count.EqualTo(originalAdjustmentCount + 1));
            Assert.That(recordedAdjustment.AdjustedAt, Is.EqualTo(expectedUtcTimestamp));
            Assert.That(recordedAdjustment.AdjustedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }
}
