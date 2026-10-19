using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

public abstract class EmployeeTestBase
{
    protected Employee _validEmployee = null!;
    protected DateTimeOffset _initialGrantedAt;

    [SetUp]
    public void Setup()
    {
        _validEmployee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        _initialGrantedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );
    }
}
