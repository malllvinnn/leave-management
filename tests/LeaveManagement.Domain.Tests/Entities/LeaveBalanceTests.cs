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
    private LeaveBalance _leaveBalance;

    [SetUp]
    public void Setup()
    {
        _employeeId = EmployeeId.New();
        _year = 2026;
        _annualQuota = LeaveDays.Create(12).Value;
        _carriedOver = LeaveDays.Create(4).Value;

        _leaveBalance = LeaveBalance.Create(
            employeeId: _employeeId,
            year: _year,
            annualQuota: _annualQuota,
            carriedOver: _carriedOver
        ).Value;
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
        var leaveBalance = _leaveBalance;
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
}