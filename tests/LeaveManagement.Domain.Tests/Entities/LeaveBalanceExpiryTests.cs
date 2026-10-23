using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class LeaveBalanceExpiryTests : LeaveBalanceTestBase
{

    [Test]
    public void ExpireCarryOver_BeforeExpiryDate_DoesNotChangeCarriedOver()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var asOfDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);
        var originalCarriedOver = leaveBalance.CarriedOver;

        // Act
        var result = leaveBalance.ExpireCarryOver(asOf: asOfDate);
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(originalCarriedOver));
        });
    }

    [Test]
    public void ExpireCarryOver_OnExpiryDate_DoesNotChangeCarriedOver()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var asOfDate = leaveBalance.CarryOverExpiresAt;
        var originalCarriedOver = leaveBalance.CarriedOver;

        // Act
        var result = leaveBalance.ExpireCarryOver(asOf: asOfDate);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(originalCarriedOver));
        });
    }

    [Test]
    public void ExpireCarryOver_AfterExpiryWithUnusedCarryOver_ReducesCarriedOverToUsedPlusReserved()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var asOfDate = leaveBalance.CarryOverExpiresAt.AddDays(1);
        var expectedCarriedOver = leaveBalance.CarryOverUsed.Add(leaveBalance.CarryOverReserved);

        // Act
        var result = leaveBalance.ExpireCarryOver(asOf: asOfDate);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(expectedCarriedOver));
            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(LeaveDays.Zero));
        });
    }

    [Test]
    public void ExpireCarryOver_AfterExpiryWithReservedCarryOver_KeepsReservedPortionIntact()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var beforeExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(-1);
        var reservedDays = LeaveDays.Create(2).Value;

        leaveBalance.Reserve(days: reservedDays, asOf: beforeExpiryDate);

        var expectedCarryOverReserved = leaveBalance.CarryOverReserved;
        var afterExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(1);

        // Act
        var result = leaveBalance.ExpireCarryOver(asOf: afterExpiryDate);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(expectedCarryOverReserved));
            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(expectedCarryOverReserved));
        });
    }

    [Test]
    public void ExpireCarryOver_CalledTwiceAfterExpiry_DoesNotChangeStateOnSecondCall()
    {
        // Arrange
        var leaveBalance = _leaveBalance;
        var afterExpiryDate = leaveBalance.CarryOverExpiresAt.AddDays(1);

        leaveBalance.ExpireCarryOver(asOf: afterExpiryDate);

        var carriedOverAfterFirstCall = leaveBalance.CarriedOver;
        var carryOverReservedAfterFirstCall = leaveBalance.CarryOverReserved;
        var carryOverUsedAfterFirstCall = leaveBalance.CarryOverUsed;

        // Act
        var result = leaveBalance.ExpireCarryOver(asOf: afterExpiryDate);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(leaveBalance.CarriedOver, Is.EqualTo(carriedOverAfterFirstCall));
            Assert.That(leaveBalance.CarryOverReserved, Is.EqualTo(carryOverReservedAfterFirstCall));
            Assert.That(leaveBalance.CarryOverUsed, Is.EqualTo(carryOverUsedAfterFirstCall));
        });
    }
}
