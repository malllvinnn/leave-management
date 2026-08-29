using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class LeaveBalanceTests
{
    private EmployeeId _employeeId;
    private int _year;
    private LeaveDays _annualQuota;
    private LeaveDays _carriedOver;

    [SetUp]
    public void Setup()
    {
        _employeeId = EmployeeId.New();
        _year = 2026;
        _annualQuota = LeaveDays.Create(12).Value;
        _carriedOver = LeaveDays.Create(4).Value;
    }

    [Test]
    public void Create_WithValidValues_ReturnsSuccessfulResultWithInitialState()
    {
        // Arrange
        var employeeId = EmployeeId.New();
        int year = 2026;
        var annualQuota = LeaveDays.Create(12).Value;
        var carriedOver = LeaveDays.Create(4).Value;

        // Act
        var result = LeaveBalance.Create(
            employeeId: employeeId,
            year: year,
            annualQuota: annualQuota,
            carriedOver: carriedOver
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.Id.Value, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Value.EmployeeId, Is.EqualTo(employeeId));
            Assert.That(result.Value.Year, Is.EqualTo(year));
            Assert.That(result.Value.AnnualQuota, Is.EqualTo(annualQuota));
            Assert.That(result.Value.CarriedOver, Is.EqualTo(carriedOver));
            Assert.That(result.Value.CarryOverExpiresAt, Is.Not.EqualTo(default(DateOnly)));

            Assert.That(result.Value.AnnualUsed, Is.EqualTo(LeaveDays.Zero));
            Assert.That(result.Value.AnnualReserved, Is.EqualTo(LeaveDays.Zero));
            Assert.That(result.Value.CarryOverUsed, Is.EqualTo(LeaveDays.Zero));
            Assert.That(result.Value.CarryOverReserved, Is.EqualTo(LeaveDays.Zero));
            Assert.That(result.Value.Adjustments, Is.Empty);
        });
    }

    [TestCase(0)]
    [TestCase(10000)]
    public void Create_WithYearOutsideSupportedRange_ThrowsArgumentOutOfRangeExceptionWithYearParamName(int value)
    {
        // Arrange
        var year = value;

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            LeaveBalance.Create(
                employeeId: _employeeId,
                year: year,
                annualQuota: _annualQuota,
                carriedOver: _carriedOver
            );
        });

        // Assert
        Assert.That(exception.ParamName, Is.EqualTo("year"));
    }

    [Test]
    public void Create_WithCarriedOverAboveSixDays_ReturnsFailedResultWithError()
    {
        // Arrange
        var carriedOverAboveSixDays = LeaveDays.Create(7).Value;

        // Act
        var result = LeaveBalance.Create(
            employeeId: _employeeId,
            year: _year,
            annualQuota: _annualQuota,
            carriedOver: carriedOverAboveSixDays
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Carried over leave cannot exceed 6 days"));
        });
    }

    [Test]
    public void Create_WithValidValues_SetsCarryOverExpiryToMarchThirtyFirstOfBalanceYear()
    {
        // Arrange
        var expectedCarryOverExpiresAt = new DateOnly(
            year: _year,
            month: 3,
            day: 31
        );

        // Act
        var result = LeaveBalance.Create(
            employeeId: _employeeId,
            year: _year,
            annualQuota: _annualQuota,
            carriedOver: _carriedOver
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.CarryOverExpiresAt, Is.EqualTo(expectedCarryOverExpiresAt));
        });
    }
}