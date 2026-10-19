using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
[TestFixture]
public class LeaveBalanceReserveTests : LeaveBalanceTestBase
{

    [Test]
    public void Reserve_WithZeroDays_ReturnsFailedResultWithError()
    {
        // Arrange
        var requestedDays = LeaveDays.Zero;
        var leaveBalance = _leaveBalance;
        var asOfDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);

        // Act
        var result = leaveBalance.Reserve(
            days: requestedDays,
            asOf: asOfDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Reserved leave days must be greater than zero"));
        });
    }

    [Test]
    public void Reserve_ExceedingAvailableBalance_ReturnsFailedResultAndDoesNotChangeAnyBucket()
    {
        // Arrange
        var requestedDays = LeaveDays.Create(20).Value;
        var leaveBalance = _leaveBalance;
        var asOfDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);

        var originalAnnualUsed = leaveBalance.AnnualUsed;
        var originalAnnualReserved = leaveBalance.AnnualReserved;
        var originalCarryOverUsed = leaveBalance.CarryOverUsed;
        var originalCarryOverReserved = leaveBalance.CarryOverReserved;

        // Act
        var result = leaveBalance.Reserve(
            days: requestedDays,
            asOf: asOfDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Insufficient leave balance for the requested days"));

            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(originalAnnualUsed));
            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(originalAnnualReserved));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(originalCarryOverUsed));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(originalCarryOverReserved));
        });
    }

    [Test]
    public void Reserve_WithinCarryOver_ReturnsAllocationFromCarryOverOnly()
    {
        // Arrange
        var requestedDays = LeaveDays.Create(4).Value;
        var leaveBalance = _leaveBalance;
        var asOfDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);

        // Act
        var result = leaveBalance.Reserve(
            days: requestedDays,
            asOf: asOfDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.CarryOver, Is.EqualTo(requestedDays));
            Assert.That(result.Value.Annual, Is.EqualTo(LeaveDays.Zero));
        });
    }

    [Test]
    public void Reserve_ExceedingCarryOver_ReturnsAllocationSplitAcrossCarryOverAndAnnual()
    {
        // Arrange
        var requestedDays = LeaveDays.Create(8).Value;
        var expectedCarryOverAllocation = _carriedOver;
        var expectedAnnualAllocation = LeaveDays.Create(4).Value;
        var leaveBalance = _leaveBalance;
        var asOfDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);

        // Act
        var result = leaveBalance.Reserve(
            days: requestedDays,
            asOf: asOfDate
        );
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.CarryOver, Is.EqualTo(expectedCarryOverAllocation));
            Assert.That(result.Value.Annual, Is.EqualTo(expectedAnnualAllocation));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(expectedCarryOverAllocation));
            Assert.That(leaveBalance.AnnualReserved, Is.EqualTo(expectedAnnualAllocation));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(LeaveDays.Zero));
            Assert.That(leaveBalance.AnnualUsed, Is.EqualTo(LeaveDays.Zero));
        });
    }

    [Test]
    public void Reserve_AfterCarryOverExpiry_ReturnsAllocationFromAnnualOnly()
    {
        // Arrange
        var requestedDays = LeaveDays.Create(8).Value;
        var leaveBalance = _leaveBalance;
        var asOfDate = leaveBalance.CarryOverExpiresAt.AddDays(1);

        // Act
        var result = leaveBalance.Reserve(
            days: requestedDays,
            asOf: asOfDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.CarryOver, Is.EqualTo(LeaveDays.Zero));
            Assert.That(result.Value.Annual, Is.EqualTo(requestedDays));
        });
    }

    [Test]
    public void Reserve_WithValidDays_ReturnsAllocationTotallingRequestedDays()
    {
        // Arrange
        var requestedDays = LeaveDays.Create(8).Value;
        var leaveBalance = _leaveBalance;
        var asOfDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);

        // Act
        var result = leaveBalance.Reserve(
            days: requestedDays,
            asOf: asOfDate
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.Total, Is.EqualTo(requestedDays));
        });
    }
}
