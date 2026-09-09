using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.ValueObjects;

[TestFixture]
public class LeaveAllocationTests
{
    [Test]
    public void Create_WithValidParts_ReturnsSuccessfulResultWithCorrectTotal()
    {
        // Arrange
        var annualDays = LeaveDays.Create(5).Value;
        var carryOverDays = LeaveDays.Create(2).Value;

        // Act
        var result = LeaveAllocation.Create(
            annual: annualDays,
            carryOver: carryOverDays
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.Total.Value, Is.EqualTo(7));
        });
    }

    [Test]
    public void Create_WithZeroTotal_ReturnsFailedResultWithError()
    {
        // Arrange
        var annualDays = LeaveDays.Zero;
        var carryOverDays = LeaveDays.Zero;

        // Act
        var result = LeaveAllocation.Create(
            annual: annualDays,
            carryOver: carryOverDays
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave allocation must contain at least one day"));
        });
    }
}