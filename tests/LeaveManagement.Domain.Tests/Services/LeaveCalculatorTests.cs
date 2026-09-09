using LeaveManagement.Domain.Services;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Services;

[TestFixture]
public class LeaveCalculatorTests
{
    private LeaveCalculator _leaveCalculator;

    [SetUp]
    public void Setup()
    {
        _leaveCalculator = new LeaveCalculator();
    }

    [Test]
    public void Calculate_WithNullPeriod_ThrowsArgumentNullExceptionWithPeriodParamName()
    {
        // Arrange
        DateRange period = null!;
        var holidays = Array.Empty<DateOnly>();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => _leaveCalculator.Calculate(
            period: period,
            holidays: holidays
        ));

        // Assert
        Assert.That(exception.ParamName, Is.EqualTo("period"));
    }

    [Test]
    public void Calculate_WithNullHolidays_ThrowsArgumentNullExceptionWithHolidaysParamName()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 12, 21),
            end: new DateOnly(2026, 12, 25)
        ).Value;

        IReadOnlyCollection<DateOnly> holidays = null!;

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => _leaveCalculator.Calculate(
            period: period,
            holidays: holidays
        ));

        // Assert
        Assert.That(exception.ParamName, Is.EqualTo("holidays"));
    }
}