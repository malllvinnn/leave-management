using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.Services;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Services;

[TestFixture]
public class RoleResolverTests
{
    private RoleResolver _roleResolver;
    private Employee _employee;
    private EmployeeId _actorId;

    [SetUp]
    public void Setup()
    {
        _roleResolver = new RoleResolver();

        _employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: null,
            positionId: null
        ).Value;

        _actorId = EmployeeId.New();
    }

    [Test]
    public void Resolve_WithNullEmployee_ThrowsArgumentNullExceptionWithEmployeeParamName()
    {
        // Arrange
        int activeSubordinateCount = 0;

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => _roleResolver.Resolve(
            employee: null!,
            activeSubordinateCount: activeSubordinateCount
        ));

        // Assert
        Assert.That(exception.ParamName, Is.EqualTo("employee"));
    }

    [Test]
    public void Resolve_WithNegativeActiveSubordinateCount_ThrowsArgumentOutOfRangeExceptionWithActiveSubordinateCountParamName()
    {
        // Arrange
        int activeSubordinateCount = -1;

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _roleResolver.Resolve(
            employee: _employee,
            activeSubordinateCount: activeSubordinateCount
        ));

        // Assert
        Assert.That(exception.ParamName, Is.EqualTo("activeSubordinateCount"));
    }

    [Test]
    public void Resolve_WithoutAdminGrantAndNoActiveSubordinate_ReturnsEmployee()
    {
        // Arrange
        var employee = _employee;
        int activeSubordinateCount = 0;
        var expectedRole = SystemRole.Employee;


        // Act
        var result = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: activeSubordinateCount
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expectedRole));
            Assert.That(employee.Role, Is.EqualTo(expectedRole));
        });
    }

    [Test]
    public void Resolve_WithoutAdminGrantAndOneActiveSubordinate_ReturnsSuperEmployee()
    {
        // Arrange
        var employee = _employee;
        int activeSubordinateCount = 1;
        var expectedRole = SystemRole.SuperEmployee;

        // Act
        var result = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: activeSubordinateCount
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expectedRole));
            Assert.That(employee.Role, Is.EqualTo(expectedRole));
        });
    }

    [Test]
    public void Resolve_WithoutAdminGrantAndManyActiveSubordinates_ReturnsSuperEmployee()
    {
        // Arrange
        var employee = _employee;
        int activeSubordinateCount = 5;
        var expectedRole = SystemRole.SuperEmployee;

        // Act
        var result = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: activeSubordinateCount
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(expectedRole));
            Assert.That(employee.Role, Is.EqualTo(expectedRole));
        });
    }

    [Test]
    public void Resolve_WithAdminGrantAndNoActiveSubordinate_ReturnsAdministrator()
    {
        // Arrange
        var employee = _employee;
        int activeSubordinateCount = 0;

        var grantedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        var grantResult = employee.GrantAdmin(_actorId, grantedAt);

        Assert.That(grantResult.IsSuccess, Is.True);

        // Act
        var result = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: activeSubordinateCount
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(SystemRole.Administrator));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.Administrator));
        });
    }

    [Test]
    public void Resolve_WithAdminGrantAndActiveSubordinates_ReturnsAdministrator()
    {
        // Arrange
        var employee = _employee;
        int activeSubordinateCount = 5;

        var initialResolve = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: activeSubordinateCount
        );

        Assert.Multiple(() =>
        {
            Assert.That(initialResolve, Is.EqualTo(SystemRole.SuperEmployee));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.SuperEmployee));
        });

        var grantedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        employee.GrantAdmin(_actorId, grantedAt);

        // Act
        var resultResolve = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: activeSubordinateCount
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(resultResolve, Is.EqualTo(SystemRole.Administrator));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.Administrator));
        });
    }
}