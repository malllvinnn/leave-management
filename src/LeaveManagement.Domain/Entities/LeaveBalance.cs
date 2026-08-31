using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Entities;

public class LeaveBalance
{
    public LeaveBalanceId Id { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public int Year { get; private set; }
    public LeaveDays AnnualQuota { get; private set; }
    public LeaveDays CarriedOver { get; private set; }
    public DateOnly CarryOverExpiresAt { get; private set; }
    public LeaveDays AnnualUsed { get; private set; }
    public LeaveDays AnnualReserved { get; private set; }
    public LeaveDays CarryOverUsed { get; private set; }
    public LeaveDays CarryOverReserved { get; private set; }

    public IReadOnlyList<QuotaAdjustment> Adjustments
    {
        get
        {
            return _adjustments;
        }
    }

    private readonly List<QuotaAdjustment> _adjustments = [];

    private LeaveBalance(
        LeaveBalanceId id,
        EmployeeId employeeId,
        int year,
        LeaveDays annualQuota,
        LeaveDays carriedOver,
        DateOnly carryOverExpiresAt
    )
    {
        Id = id;
        EmployeeId = employeeId;
        Year = year;
        AnnualQuota = annualQuota;
        CarriedOver = carriedOver;
        CarryOverExpiresAt = carryOverExpiresAt;

        AnnualUsed = LeaveDays.Zero;
        AnnualReserved = LeaveDays.Zero;
        CarryOverUsed = LeaveDays.Zero;
        CarryOverReserved = LeaveDays.Zero;
    }

    public static Result<LeaveBalance> Create(
        EmployeeId employeeId,
        int year,
        LeaveDays annualQuota,
        LeaveDays carriedOver
    )
    {
        if (year is < 1 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }

        if (carriedOver.Value > 6)
        {
            var failureResult = Result<LeaveBalance>.Fail("Carried over leave cannot exceed 6 days");

            return failureResult;
        }

        var carryOverExpiresAt = new DateOnly(
            year: year,
            month: 3,
            day: 31
        );

        var leaveBalance = new LeaveBalance(
            id: LeaveBalanceId.New(),
            employeeId: employeeId,
            year: year,
            annualQuota: annualQuota,
            carriedOver: carriedOver,
            carryOverExpiresAt: carryOverExpiresAt
        );

        var successResult = Result<LeaveBalance>.Ok(leaveBalance);

        return successResult;
    }

    public static Result<LeaveDays> CalculateProratedQuota(DateOnly hireDate, int year)
    {
        if (year is < 1 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }

        if (hireDate.Year > year)
        {
            var failureResult = Result<LeaveDays>.Fail("Hire date cannot be later than the balance year");

            return failureResult;
        }

        const int annualQuotaDays = 12;
        const int monthsInYear = 12;
        const int fullMonthCutoffDay = 15;

        if (hireDate.Year < year)
        {
            var fullAnnualQuotaResult = LeaveDays.Create(annualQuotaDays);

            return fullAnnualQuotaResult;
        }

        var remainingMonthsAfterHireMonth = monthsInYear - hireDate.Month;
        var includesHireMonth = hireDate.Day <= fullMonthCutoffDay ? 1 : 0;
        var remainingFullMonths = remainingMonthsAfterHireMonth + includesHireMonth;
        var proratedQuotaValue = annualQuotaDays * remainingFullMonths / monthsInYear;

        var proratedQuotaResult = LeaveDays.Create(proratedQuotaValue);

        return proratedQuotaResult;
    }

    public static LeaveDays CalculateCarryOver(LeaveDays remaining)
    {
        var carryOverValue = Math.Min(remaining.Value, 6);
        var carryOverResult = LeaveDays.Create(carryOverValue).Value;

        return carryOverResult;
    }

    public LeaveDays Available(DateOnly asOf)
    {
        var annualAvailable = AnnualQuota.Value - AnnualUsed.Value - AnnualReserved.Value;

        if (asOf > CarryOverExpiresAt)
        {
            var availableResult = LeaveDays.Create(annualAvailable).Value;

            return availableResult;
        }

        var carryOverAvailable = CarriedOver.Value - CarryOverUsed.Value - CarryOverReserved.Value;
        var totalAvailable = annualAvailable + carryOverAvailable;

        var totalAvailableResult = LeaveDays.Create(totalAvailable).Value;

        return totalAvailableResult;
    }

    public Result<LeaveAllocation> Reserve(LeaveDays days, DateOnly asOf)
    {
        if (days == LeaveDays.Zero)
        {
            var failureResult = Result<LeaveAllocation>.Fail("Reserved leave days must be greater than zero");

            return failureResult;
        }

        var availableDays = Available(asOf);

        if (days.Value > availableDays.Value)
        {
            var failureResult = Result<LeaveAllocation>.Fail("Insufficient leave balance for the requested days");

            return failureResult;
        }

        var carryOverAvailable = asOf > CarryOverExpiresAt
            ? 0
            : CarriedOver.Value - CarryOverUsed.Value - CarryOverReserved.Value;

        var carryOverToReserveValue = Math.Min(
            days.Value,
            carryOverAvailable
        );

        var annualToReserveValue = days.Value - carryOverToReserveValue;

        var carryOverToReserve = LeaveDays.Create(carryOverToReserveValue).Value;
        var annualToReserve = LeaveDays.Create(annualToReserveValue).Value;

        var allocationResult = LeaveAllocation.Create(
            annual: annualToReserve,
            carryOver: carryOverToReserve
        );

        AnnualReserved = AnnualReserved.Add(annualToReserve);
        CarryOverReserved = CarryOverReserved.Add(carryOverToReserve);

        return allocationResult;
    }

    public Result ReleaseReservation(LeaveAllocation allocation, DateOnly asOf)
    {
        throw new NotImplementedException();
    }

    public Result ConfirmUsage(LeaveAllocation allocation)
    {
        throw new NotImplementedException();
    }

    public Result CancelUsage(LeaveAllocation allocation, DateOnly asOf)
    {
        throw new NotImplementedException();
    }

    public Result Adjust(
        int days,
        Reason? reason,
        EmployeeId actorId,
        DateTimeOffset now
    )
    {
        throw new NotImplementedException();
    }

    public Result ExpireCarryOver(DateOnly asOf)
    {
        throw new NotImplementedException();
    }
}