using LeaveManagement.Domain.Services;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Services;

[TestFixture]
public class LeaveCalculatorTests
{
    private LeaveCalculator _leaveCalculator;
    private IReadOnlyCollection<DateOnly> _emptyHolidays;

    [SetUp]
    public void Setup()
    {
        _leaveCalculator = new LeaveCalculator();
        _emptyHolidays = Array.Empty<DateOnly>();
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

    [Test]
    public void Calculate_WithMondayToFridayAndNoHolidays_ReturnsFiveDays()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 12, 21),
            end: new DateOnly(2026, 12, 25)
        ).Value;

        // Act
        var result = _leaveCalculator.Calculate(
            period: period,
            holidays: _emptyHolidays
        );

        // Assert
        Assert.That(result.Value, Is.EqualTo(5));
    }

    [Test]
    public void Calculate_WithSingleWeekday_ReturnsOneDay()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 12, 23),
            end: new DateOnly(2026, 12, 23)
        ).Value;

        // Act
        var result = _leaveCalculator.Calculate(
            period: period,
            holidays: _emptyHolidays
        );

        // Assert
        Assert.That(result.Value, Is.EqualTo(1));
    }

    [Test]
    public void Calculate_WithRangeSpanningWeekend_ExcludesSaturdayAndSunday()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 12, 18),
            end: new DateOnly(2026, 12, 21)
        ).Value;

        // Act
        var result = _leaveCalculator.Calculate(
            period: period,
            holidays: _emptyHolidays
        );

        // Assert
        Assert.That(result.Value, Is.EqualTo(2));
    }

    [Test]
    public void Calculate_WithRangeEntirelyOnWeekend_ReturnsZeroDays()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 12, 19),
            end: new DateOnly(2026, 12, 20)
        ).Value;

        // Act
        var result = _leaveCalculator.Calculate(
            period: period,
            holidays: _emptyHolidays
        );

        // Assert
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [TestCase(21)]
    [TestCase(23)]
    [TestCase(25)]
    public void Calculate_WithHolidayOnWeekday_ExcludesThatDay(int value)
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 12, 21),
            end: new DateOnly(2026, 12, 25)
        ).Value;

        var holidays = new List<DateOnly>
        {
            new DateOnly(2026, 12, value)
        };

        // Act
        var result = _leaveCalculator.Calculate(
            period: period,
            holidays: holidays
        );

        // Assert
        Assert.That(result.Value, Is.EqualTo(4));
    }

    [Test]
    public void Calculate_WithSingleWeekdayHoliday_ReturnsZeroDays()
    {
        // Arrange
        var period = DateRange.Create(
            start: new DateOnly(2026, 12, 23),
            end: new DateOnly(2026, 12, 23)
        ).Value;

        var holidays = new List<DateOnly>
        {
            new DateOnly(2026, 12, 23)
        };

        // Act
        var result = _leaveCalculator.Calculate(
            period: period,
            holidays: holidays
        );

        // Assert
        Assert.That(result.Value, Is.EqualTo(0));
    }
}