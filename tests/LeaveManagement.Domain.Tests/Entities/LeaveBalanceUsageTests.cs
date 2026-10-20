using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class LeaveBalanceUsageTests : LeaveBalanceTestBase
{

    [Test]
    public void ReleaseReservation_BeforeExpiry_RestoresEachBucketExactly()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);
        var requestedDays = LeaveDays.Create(6).Value;

        var originalCarriedOver = leaveBalance.CarriedOver;
        var originalAnnualUsed = leaveBalance.AnnualUsed;
        var originalAnnualReserved = leaveBalance.AnnualReserved;
        var originalCarryOverUsed = leaveBalance.CarryOverUsed;
        var originalCarryOverReserved = leaveBalance.CarryOverReserved;
        var originalAvailable = leaveBalance.Available(asOf: beforeExpiryDate);

        var reserveResult = leaveBalance.Reserve(
            days: requestedDays,
            asOf: beforeExpiryDate
        );

        Assert.That(reserveResult.IsSuccess, Is.True);

        // Act
        var result = leaveBalance.ReleaseReservation(
            allocation: reserveResult.Value,
            asOf: beforeExpiryDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(originalCarriedOver));
            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(originalAnnualUsed));
            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(originalAnnualReserved));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(originalCarryOverUsed));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(originalCarryOverReserved));
            Assert.That(leaveBalance.Available(asOf: beforeExpiryDate), Is.EqualTo(originalAvailable));
        });
    }

    [Test]
    public void ReleaseReservation_AfterExpiry_DoesNotRestoreExpiredCarryOver()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);
        var afterExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(1);
        var requestedDays = LeaveDays.Create(2).Value;

        var originalAnnualReserved = leaveBalance.AnnualReserved;

        var reserveResult = leaveBalance.Reserve(
            days: requestedDays,
            asOf: beforeExpiryDate
        );

        Assert.That(reserveResult.IsSuccess, Is.True);

        var originalAvailable = leaveBalance.Available(asOf: afterExpiryDate);

        // Act
        var result = leaveBalance.ReleaseReservation(
            allocation: reserveResult.Value,
            asOf: afterExpiryDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(originalAnnualReserved));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(LeaveDays.Zero));
            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(LeaveDays.Create(2).Value));
            Assert.That(leaveBalance.Available(asOf: afterExpiryDate), Is.EqualTo(originalAvailable));
        });
    }

    [Test]
    public void ReleaseReservation_ExceedingReservedBucket_ReturnsFailedResultAndDoesNotChangeAnyBucket()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);
        var reservedDays = LeaveDays.Create(6).Value;

        var reserveResult = leaveBalance.Reserve(
            days: reservedDays,
            asOf: beforeExpiryDate
        );

        Assert.That(reserveResult.IsSuccess, Is.True);

        var allocationResult = LeaveAllocation.Create(
            annual: LeaveDays.Create(1).Value,
            carryOver: LeaveDays.Create(5).Value
        );

        Assert.That(allocationResult.IsSuccess, Is.True);

        var originalCarriedOver = leaveBalance.CarriedOver;
        var originalAnnualUsed = leaveBalance.AnnualUsed;
        var originalAnnualReserved = leaveBalance.AnnualReserved;
        var originalCarryOverUsed = leaveBalance.CarryOverUsed;
        var originalCarryOverReserved = leaveBalance.CarryOverReserved;

        // Act
        var result = leaveBalance.ReleaseReservation(
            allocation: allocationResult.Value,
            asOf: beforeExpiryDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Released allocation exceeds the reserved balance"));

            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(originalCarriedOver));
            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(originalAnnualUsed));
            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(originalAnnualReserved));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(originalCarryOverUsed));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(originalCarryOverReserved));
        });
    }

    [Test]
    public void ConfirmUsage_WithMatchingAllocation_MovesReservedToUsedPerSource()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);
        var reservedDays = LeaveDays.Create(6).Value;

        var reserveResult = leaveBalance.Reserve(
            days: reservedDays,
            asOf: beforeExpiryDate
        );

        Assert.Multiple(() =>
        {
            Assert.That(reserveResult.IsSuccess, Is.True);
            Assert.That(leaveBalance.AnnualReserved, Is.Not.EqualTo(LeaveDays.Zero));
            Assert.That(leaveBalance.CarryOverReserved, Is.Not.EqualTo(LeaveDays.Zero));
            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(LeaveDays.Zero));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(LeaveDays.Zero));
        });

        var allocation = reserveResult.Value;

        // Act
        var result = leaveBalance.ConfirmUsage(allocation: allocation);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(LeaveDays.Zero));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(LeaveDays.Zero));
            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(allocation.Annual));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(allocation.CarryOver));
        });
    }

    [Test]
    public void ConfirmUsage_ExceedingReservedBucket_ReturnsFailedResultAndDoesNotChangeAnyBucket()
    {
        // Arrange
        var leaveBalance = _leaveBalance; // carryOver setup 4
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);
        var reservedDays = LeaveDays.Create(6).Value;

        var reserveResult = leaveBalance.Reserve(
            days: reservedDays,
            asOf: beforeExpiryDate
        );

        Assert.That(reserveResult.IsSuccess, Is.True);

        var excessiveAllocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(1).Value, // within limit
            carryOver: LeaveDays.Create(5).Value // exceed 4
        ).Value;

        var originalAnnualUsed = leaveBalance.AnnualUsed;
        var originalAnnualReserved = leaveBalance.AnnualReserved;
        var originalCarryOverUsed = leaveBalance.CarryOverUsed;
        var originalCarryOverReserved = leaveBalance.CarryOverReserved;

        // Act
        var result = leaveBalance.ConfirmUsage(allocation: excessiveAllocation);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Confirmed allocation exceeds the reserved balance"));

            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(originalAnnualUsed));
            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(originalAnnualReserved));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(originalCarryOverUsed));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(originalCarryOverReserved));
        });
    }

    [Test]
    public void ConfirmUsage_AfterCarryOverExpiry_ConfirmsCarryOverPortion()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);
        var afterExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(1);
        var reservedDays = LeaveDays.Create(2).Value;

        var reserveResult = leaveBalance.Reserve(
            days: reservedDays,
            asOf: beforeExpiryDate
        );

        Assert.That(reserveResult.IsSuccess, Is.True);

        var allocation = reserveResult.Value;

        var expireResult = leaveBalance.ExpireCarryOver(asOf: afterExpiryDate);

        Assert.That(expireResult.IsSuccess, Is.True);

        // Act
        var result = leaveBalance.ConfirmUsage(allocation: allocation);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(LeaveDays.Zero));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(allocation.CarryOver));
            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(LeaveDays.Zero));
            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(LeaveDays.Zero));
        });
    }

    [Test]
    public void CancelUsage_BeforeExpiry_RestoresUsedPerSource()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);

        // carry-over 4 and annual 2
        var requestedDays = LeaveDays.Create(6).Value;

        var originalAnnualUsed = leaveBalance.AnnualUsed;
        var originalCarryOverUsed = leaveBalance.CarryOverUsed;
        var originalCarriedOver = leaveBalance.CarriedOver;
        var originalAvailable = leaveBalance.Available(beforeExpiryDate);

        var reserveResult = leaveBalance.Reserve(
            days: requestedDays,
            asOf: beforeExpiryDate
        );

        Assert.That(reserveResult.IsSuccess, Is.True);

        var allocation = reserveResult.Value;

        Assert.Multiple(() =>
        {
            Assert.That(allocation.Annual, Is.Not.EqualTo(LeaveDays.Zero));
            Assert.That(allocation.CarryOver, Is.Not.EqualTo(LeaveDays.Zero));
        });

        var confirmUsageResult = leaveBalance.ConfirmUsage(allocation);

        Assert.That(confirmUsageResult.IsSuccess, Is.True);

        var annualQuotaBeforeCancel = leaveBalance.AnnualQuota;
        var annualReservedBeforeCancel = leaveBalance.AnnualReserved;
        var carryOverReservedBeforeCancel = leaveBalance.CarryOverReserved;

        // Act
        var result = leaveBalance.CancelUsage(
            allocation: allocation,
            asOf: beforeExpiryDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.AnnualQuota, Is.EqualTo(annualQuotaBeforeCancel));
            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(originalAnnualUsed));
            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(annualReservedBeforeCancel));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(originalCarryOverUsed));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(carryOverReservedBeforeCancel));
            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(originalCarriedOver));
            Assert.That(leaveBalance.Available(beforeExpiryDate), Is.EqualTo(originalAvailable));
        });
    }

    [Test]
    public void CancelUsage_AfterExpiry_DoesNotRestoreExpiredCarryOver()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);
        var afterExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(1);

        // from carry-over only (4 - 2)
        var requestedDays = LeaveDays.Create(2).Value;

        var reserveResult = leaveBalance.Reserve(
            days: requestedDays,
            asOf: beforeExpiryDate
        );

        Assert.That(reserveResult.IsSuccess, Is.True);

        var allocation = reserveResult.Value;

        Assert.Multiple(() =>
        {
            Assert.That(allocation.Annual, Is.EqualTo(LeaveDays.Zero));
            Assert.That(allocation.CarryOver, Is.EqualTo(requestedDays));
        });

        var confirmUsageResult = leaveBalance.ConfirmUsage(allocation);

        Assert.That(confirmUsageResult.IsSuccess, Is.True);

        var annualQuotaBeforeCancel = leaveBalance.AnnualQuota;
        var annualUsedBeforeCancel = leaveBalance.AnnualUsed;
        var annualReservedBeforeCancel = leaveBalance.AnnualReserved;
        var carryOverReservedBeforeCancel = leaveBalance.CarryOverReserved;
        var carriedOverBeforeCancel = leaveBalance.CarriedOver;
        var availableBeforeCancel = leaveBalance.Available(afterExpiryDate);
        var expectedCarriedOver = carriedOverBeforeCancel.Subtract(allocation.CarryOver).Value;

        // Act
        var result = leaveBalance.CancelUsage(
            allocation: allocation,
            asOf: afterExpiryDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.AnnualQuota, Is.EqualTo(annualQuotaBeforeCancel));
            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(annualUsedBeforeCancel));
            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(annualReservedBeforeCancel));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(LeaveDays.Zero));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(carryOverReservedBeforeCancel));
            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(expectedCarriedOver));
            Assert.That(leaveBalance.Available(afterExpiryDate), Is.EqualTo(availableBeforeCancel));
        });
    }

    [TestCase(1, 5)]
    [TestCase(3, 3)]
    public void CancelUsage_ExceedingUsedBucket_ReturnsFailedResultAndDoesNotChangeAnyBucket(
        int annualDays,
        int carryOverDays)
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);

        // carry-over 4 and annual 2
        var requestedDays = LeaveDays.Create(6).Value;

        var reserveResult = leaveBalance.Reserve(
            days: requestedDays,
            asOf: beforeExpiryDate
        );

        Assert.That(reserveResult.IsSuccess, Is.True);

        var confirmResult = leaveBalance.ConfirmUsage(reserveResult.Value);

        Assert.That(confirmResult.IsSuccess, Is.True);

        // Existing used buckets: annual 2 and carry-over 4.
        // Each case exceeds one source while the other remains sufficient.
        var excessiveAllocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(annualDays).Value,
            carryOver: LeaveDays.Create(carryOverDays).Value
        ).Value;

        var originalAnnualQuota = leaveBalance.AnnualQuota;
        var originalAnnualUsed = leaveBalance.AnnualUsed;
        var originalAnnualReserved = leaveBalance.AnnualReserved;
        var originalCarryOverUsed = leaveBalance.CarryOverUsed;
        var originalCarryOverReserved =
            leaveBalance.CarryOverReserved;

        var originalCarriedOver = leaveBalance.CarriedOver;

        // Act
        var result = leaveBalance.CancelUsage(
            allocation: excessiveAllocation,
            asOf: beforeExpiryDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Cancelled allocation exceeds the used balance"));

            Assert.That(leaveBalance.AnnualQuota, Is.EqualTo(originalAnnualQuota));
            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(originalAnnualUsed));
            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(originalAnnualReserved));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(originalCarryOverUsed));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(originalCarryOverReserved));
            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(originalCarriedOver));
        });
    }
}
