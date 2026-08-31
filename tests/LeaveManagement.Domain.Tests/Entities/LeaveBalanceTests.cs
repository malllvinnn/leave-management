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

    [Test]
    public void CalculateProratedQuota_WithHireDateInJanuary_ReturnsSuccessfulResultWithTwelveDays()
    {
        // Arrange
        var hireDate = new DateOnly(year: _year, month: 1, day: 1);
        var expectedProratedQuota = LeaveDays.Create(12).Value;

        // Act
        var result = LeaveBalance.CalculateProratedQuota(
            hireDate: hireDate,
            year: _year
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value, Is.EqualTo(expectedProratedQuota));
        });
    }

    [Test]
    public void CalculateProratedQuota_WithHireDateOnTenthOfMarch_ReturnsSuccessfulResultWithTenDays()
    {
        // Arrange
        var hireDate = new DateOnly(year: _year, month: 3, day: 10);
        var expectedProratedQuota = LeaveDays.Create(10).Value;

        // Act
        var result = LeaveBalance.CalculateProratedQuota(
            hireDate: hireDate,
            year: _year
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value, Is.EqualTo(expectedProratedQuota));
        });
    }

    [Test]
    public void CalculateProratedQuota_WithHireDateOnFifteenthOfMarch_ReturnsSuccessfulResultWithTenDays()
    {
        // Arrange
        var hireDate = new DateOnly(year: _year, month: 3, day: 15);
        var expectedProratedQuota = LeaveDays.Create(10).Value;

        // Act
        var result = LeaveBalance.CalculateProratedQuota(
            hireDate: hireDate,
            year: _year
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value, Is.EqualTo(expectedProratedQuota));
        });
    }

    [Test]
    public void CalculateProratedQuota_WithHireDateOnTwentiethOfMarch_ReturnsSuccessfulResultWithNineDays()
    {
        // Arrange
        var hireDate = new DateOnly(year: _year, month: 3, day: 20);
        var expectedProratedQuota = LeaveDays.Create(9).Value;

        // Act
        var result = LeaveBalance.CalculateProratedQuota(
            hireDate: hireDate,
            year: _year
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value, Is.EqualTo(expectedProratedQuota));
        });
    }

    [Test]
    public void CalculateProratedQuota_WithHireDateInSeptember_ReturnsSuccessfulResultWithFourDays()
    {
        // Arrange
        var hireDate = new DateOnly(year: _year, month: 9, day: 1);
        var expectedProratedQuota = LeaveDays.Create(4).Value;

        // Act
        var result = LeaveBalance.CalculateProratedQuota(
            hireDate: hireDate,
            year: _year
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value, Is.EqualTo(expectedProratedQuota));
        });
    }

    [Test]
    public void CalculateProratedQuota_WithHireDateOnFifteenthOfDecember_ReturnsSuccessfulResultWithOneDay()
    {
        // Arrange
        var hireDate = new DateOnly(year: _year, month: 12, day: 15);
        var expectedProratedQuota = LeaveDays.Create(1).Value;

        // Act
        var result = LeaveBalance.CalculateProratedQuota(
            hireDate: hireDate,
            year: _year
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value, Is.EqualTo(expectedProratedQuota));
        });
    }

    [Test]
    public void CalculateProratedQuota_WithHireDateAfterFifteenthOfDecember_ReturnsSuccessfulResultWithZeroDays()
    {
        // Arrange
        var hireDate = new DateOnly(year: _year, month: 12, day: 16);
        var expectedProratedQuota = LeaveDays.Create(0).Value;

        // Act
        var result = LeaveBalance.CalculateProratedQuota(
            hireDate: hireDate,
            year: _year
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value, Is.EqualTo(expectedProratedQuota));
        });
    }

    [Test]
    public void CalculateProratedQuota_WithHireDateInPreviousYear_ReturnsSuccessfulResultWithTwelveDays()
    {
        // Arrange
        var hireDate = new DateOnly(year: _year - 1, month: 12, day: 31);
        var expectedProratedQuota = LeaveDays.Create(12).Value;

        // Act
        var result = LeaveBalance.CalculateProratedQuota(
            hireDate: hireDate,
            year: _year
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value, Is.EqualTo(expectedProratedQuota));
        });
    }

    [Test]
    public void CalculateProratedQuota_WithHireDateAfterBalanceYear_ReturnsFailedResultWithError()
    {
        // Arrange
        var hireDate = new DateOnly(year: _year + 1, month: 1, day: 1);

        // Act
        var result = LeaveBalance.CalculateProratedQuota(
            hireDate: hireDate,
            year: _year
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Hire date cannot be later than the balance year"));
        });
    }

    [TestCase(0)]
    [TestCase(10000)]
    public void CalculateProratedQuota_WithYearOutsideSupportedRange_ThrowsArgumentOutOfRangeExceptionWithYearParamName(int value)
    {
        // Arrange
        var hireDate = new DateOnly(year: _year, month: 1, day: 1);
        var year = value;

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            LeaveBalance.CalculateProratedQuota(
                hireDate: hireDate,
                year: year
            );
        });

        // Assert
        Assert.That(exception.ParamName, Is.EqualTo("year"));
    }

    [TestCase(8)]
    [TestCase(12)]
    public void CalculateCarryOver_WithRemainingAboveCap_ReturnsSixDays(int value)
    {
        // Arrange
        var remainingDays = value;
        var remaining = LeaveDays.Create(remainingDays).Value;
        var expectedCarryOverSixDays = LeaveDays.Create(6).Value;

        // Act
        var result = LeaveBalance.CalculateCarryOver(remaining);

        // Assert
        Assert.That(result, Is.EqualTo(expectedCarryOverSixDays));
    }

    [Test]
    public void CalculateCarryOver_WithRemainingBelowCap_ReturnsRemainingDays()
    {
        // Arrange
        var remainingBelowCap = LeaveDays.Create(4).Value;

        // Act
        var result = LeaveBalance.CalculateCarryOver(remainingBelowCap);

        // Assert
        Assert.That(result, Is.EqualTo(remainingBelowCap));
    }

    [Test]
    public void CalculateCarryOver_WithRemainingExactlyAtCap_ReturnsSixDays()
    {
        // Arrange
        var remainingExactlyAtCap = LeaveDays.Create(6).Value;
        var expectedCarryOverSixDays = LeaveDays.Create(6).Value;

        // Act
        var result = LeaveBalance.CalculateCarryOver(remainingExactlyAtCap);

        // Assert
        Assert.That(result, Is.EqualTo(expectedCarryOverSixDays));
    }

    [Test]
    public void CalculateCarryOver_WithZeroRemaining_ReturnsZeroDays()
    {
        // Arrange
        var remainingZero = LeaveDays.Zero;
        var expectedCarryOverZeroDays = LeaveDays.Zero;

        // Act
        var result = LeaveBalance.CalculateCarryOver(remainingZero);

        // Assert
        Assert.That(result, Is.EqualTo(expectedCarryOverZeroDays));
    }

    [Test]
    public void Available_BeforeCarryOverExpiry_ReturnsEighteenDays()
    {
        // Arrange
        var annualQuota = LeaveDays.Create(12).Value;
        var carriedOver = LeaveDays.Create(6).Value;
        var expectedAvailableDays = LeaveDays.Create(18).Value;

        var leaveBalance = LeaveBalance.Create(
            employeeId: _employeeId,
            year: _year,
            annualQuota: annualQuota,
            carriedOver: carriedOver
        ).Value;

        var asOfDateBeforeExpiry = leaveBalance.CarryOverExpiresAt.AddDays(-1);

        // Act
        var result = leaveBalance.Available(asOf: asOfDateBeforeExpiry);

        // Assert
        Assert.That(result, Is.EqualTo(expectedAvailableDays));
    }

    [Test]
    public void Available_OnCarryOverExpiryDate_ReturnsEighteenDays()
    {
        // Arrange
        var annualQuota = LeaveDays.Create(12).Value;
        var carriedOver = LeaveDays.Create(6).Value;
        var expectedAvailableDays = LeaveDays.Create(18).Value;

        var leaveBalance = LeaveBalance.Create(
            employeeId: _employeeId,
            year: _year,
            annualQuota: annualQuota,
            carriedOver: carriedOver
        ).Value;

        var asOfDateOnExpiry = leaveBalance.CarryOverExpiresAt;

        // Act
        var result = leaveBalance.Available(asOf: asOfDateOnExpiry);

        // Assert
        Assert.That(result, Is.EqualTo(expectedAvailableDays));
    }

    [Test]
    public void Available_AfterCarryOverExpiry_ReturnsTwelveDays()
    {
        // Arrange
        var annualQuota = LeaveDays.Create(12).Value;
        var carriedOver = LeaveDays.Create(6).Value;
        var expectedAvailableDays = LeaveDays.Create(12).Value;

        var leaveBalance = LeaveBalance.Create(
            employeeId: _employeeId,
            year: _year,
            annualQuota: annualQuota,
            carriedOver: carriedOver
        ).Value;

        var asOfDateAfterExpiry = leaveBalance.CarryOverExpiresAt.AddDays(1);

        // Act
        var result = leaveBalance.Available(asOf: asOfDateAfterExpiry);

        // Assert
        Assert.That(result, Is.EqualTo(expectedAvailableDays));
    }
}