using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public abstract class LeaveBalanceTestBase
{
    protected EmployeeId _employeeId;
    protected int _year;
    protected LeaveDays _annualQuota;
    protected LeaveDays _carriedOver;
    protected LeaveBalance _leaveBalance;
    protected Reason _reasonAdjustment;
    protected DateTimeOffset _dateTimeNow;

    [SetUp]
    public void Setup()
    {
        _employeeId = EmployeeId.New();
        _year = DateTime.UtcNow.Year;
        _annualQuota = LeaveDays.Create(12).Value;
        _carriedOver = LeaveDays.Create(4).Value;

        _leaveBalance = LeaveBalance.Create(
            employeeId: _employeeId,
            year: _year,
            annualQuota: _annualQuota,
            carriedOver: _carriedOver
        ).Value;

        _reasonAdjustment = Reason.Create("Annual quota adjustment").Value;

        _dateTimeNow = new DateTimeOffset(
            year: _year,
            month: 9,
            day: 8,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.Zero
        );
    }
}
