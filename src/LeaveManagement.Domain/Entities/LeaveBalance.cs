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
        throw new NotImplementedException();
    }

    public static Result<LeaveDays> CalculateProratedQuota(DateOnly hireDate, int year)
    {
        throw new NotImplementedException();
    }

    public static LeaveDays CalculateCarryOver(LeaveDays remaining)
    {
        throw new NotImplementedException();
    }

    public LeaveDays Available(DateOnly asOf)
    {
        throw new NotImplementedException();
    }

    public Result<LeaveAllocation> Reserve(LeaveDays days, DateOnly asOf)
    {
        throw new NotImplementedException();
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