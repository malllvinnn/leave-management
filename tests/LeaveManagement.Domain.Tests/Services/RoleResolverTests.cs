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
    private DateTimeOffset _grantedAt;

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

        _grantedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );
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

        var grantResult = employee.GrantAdmin(_actorId, _grantedAt);

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

        employee.GrantAdmin(_actorId, _grantedAt);

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

    [Test]
    public void Resolve_AfterActiveSubordinateCountDropsToZero_DowngradesSuperEmployeeToEmployee()
    {
        // Arrange
        var employee = _employee;
        int initialActiveSubordinateCount = 1;

        var initialResolve = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: initialActiveSubordinateCount
        );

        Assert.Multiple(() =>
        {
            Assert.That(initialResolve, Is.EqualTo(SystemRole.SuperEmployee));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.SuperEmployee));
        });

        // Act
        int zeroActiveSubordinateCount = 0;

        var resultResolve = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: zeroActiveSubordinateCount
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(resultResolve, Is.EqualTo(SystemRole.Employee));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.Employee));
        });
    }

    [Test]
    public void Resolve_AfterAdminRevokedWithActiveSubordinates_DowngradesAdministratorToSuperEmployee()
    {
        // Arrange
        var employee = _employee;
        int initialActiveSubordinateCount = 1;

        var initialGranted = employee.GrantAdmin(_actorId, _grantedAt);

        var initialResolve = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: initialActiveSubordinateCount
        );

        Assert.Multiple(() =>
        {
            Assert.That(initialGranted.IsSuccess, Is.True);

            Assert.That(initialResolve, Is.EqualTo(SystemRole.Administrator));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.Administrator));
        });

        // Act
        var resultRevoke = employee.RevokeAdmin(_actorId);
        var resultResolve = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: initialActiveSubordinateCount
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(resultRevoke.IsSuccess, Is.True);

            Assert.That(resultResolve, Is.EqualTo(SystemRole.SuperEmployee));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.SuperEmployee));
        });
    }

    [Test]
    public void Resolve_AfterAdminRevokedWithoutActiveSubordinates_DowngradesAdministratorToEmployee()
    {
        // Arrange
        var employee = _employee;
        int initialActiveSubordinateCount = 0;

        var initialGranted = employee.GrantAdmin(_actorId, _grantedAt);

        var initialResolve = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: initialActiveSubordinateCount
        );

        Assert.Multiple(() =>
        {
            Assert.That(initialGranted.IsSuccess, Is.True);

            Assert.That(initialResolve, Is.EqualTo(SystemRole.Administrator));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.Administrator));
        });

        // Act
        var resultRevoke = employee.RevokeAdmin(_actorId);
        var resultResolve = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: initialActiveSubordinateCount
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(resultRevoke.IsSuccess, Is.True);

            Assert.That(resultResolve, Is.EqualTo(SystemRole.Employee));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.Employee));
        });
    }

    [Test]
    public void Resolve_AfterPositionChanged_ReturnsSameRole()
    {
        // Arrange
        var employee = _employee;
        int activeSubordinateCount = 0;
        var expectedRole = SystemRole.Employee;
        var positionId = PositionId.New();

        employee.AssignPosition(positionId);

        // Act
        var roleAfterPositionAssigned = _roleResolver.Resolve(
            employee,
            activeSubordinateCount
        );

        employee.AssignPosition(null);

        var roleAfterPositionCleared = _roleResolver.Resolve(
            employee,
            activeSubordinateCount
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(roleAfterPositionAssigned, Is.EqualTo(expectedRole));
            Assert.That(roleAfterPositionCleared, Is.EqualTo(expectedRole));
            Assert.That(employee.PositionId, Is.Null);
            Assert.That(employee.Role, Is.EqualTo(expectedRole));
        });
    }

    [Test]
    public void Resolve_WithValidInput_DoesNotModifyEmployeeFieldsOtherThanRole()
    {
        // Arrange
        var employee = _employee;
        int initialActiveSubordinateCount = 1;

        var idEmployee = employee.Id;
        var fullNameEmployee = employee.FullName;
        var emailEmployee = employee.Email;
        var hireDateEmployee = employee.HireDate;
        var managerIdEmployee = employee.ManagerId;
        var positionIdEmployee = employee.PositionId;
        var adminGrantedAtEmployee = employee.AdminGrantedAt;
        var isActiveEmployee = employee.IsActive;

        var initialResolve = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: initialActiveSubordinateCount
        );

        Assert.Multiple(() =>
        {
            Assert.That(employee.Id, Is.EqualTo(idEmployee));
            Assert.That(employee.FullName, Is.EqualTo(fullNameEmployee));
            Assert.That(employee.Email, Is.EqualTo(emailEmployee));
            Assert.That(employee.HireDate, Is.EqualTo(hireDateEmployee));
            Assert.That(employee.ManagerId, Is.EqualTo(managerIdEmployee));
            Assert.That(employee.PositionId, Is.EqualTo(positionIdEmployee));
            Assert.That(employee.AdminGrantedAt, Is.EqualTo(adminGrantedAtEmployee));
            Assert.That(employee.IsActive, Is.EqualTo(isActiveEmployee));

            Assert.That(initialResolve, Is.EqualTo(SystemRole.SuperEmployee));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.SuperEmployee));
        });

        // Act
        int downgradeActiveSubordinateCount = 0;

        var resultResolve = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: downgradeActiveSubordinateCount
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(employee.Id, Is.EqualTo(idEmployee));
            Assert.That(employee.FullName, Is.EqualTo(fullNameEmployee));
            Assert.That(employee.Email, Is.EqualTo(emailEmployee));
            Assert.That(employee.HireDate, Is.EqualTo(hireDateEmployee));
            Assert.That(employee.ManagerId, Is.EqualTo(managerIdEmployee));
            Assert.That(employee.PositionId, Is.EqualTo(positionIdEmployee));
            Assert.That(employee.AdminGrantedAt, Is.EqualTo(adminGrantedAtEmployee));
            Assert.That(employee.IsActive, Is.EqualTo(isActiveEmployee));

            Assert.That(resultResolve, Is.EqualTo(SystemRole.Employee));
            Assert.That(employee.Role, Is.EqualTo(SystemRole.Employee));
        });
    }

    [Test]
    public void EmployeeBehaviors_WhenInvokedOutsideRoleResolver_DoNotChangeRole()
    {
        // Arrange
        var employee = _employee;
        var initialRole = employee.Role;
        var managerId = EmployeeId.New();
        var positionId = PositionId.New();

        // Act
        employee.AssignManager(managerId);
        var roleAfterAssignManager = employee.Role;

        employee.AssignPosition(positionId);
        var roleAfterAssignPosition = employee.Role;

        employee.GrantAdmin(_actorId, _grantedAt);
        var roleAfterGrantAdmin = employee.Role;

        employee.RevokeAdmin(_actorId);
        var roleAfterRevokeAdmin = employee.Role;

        employee.Deactivate();
        var roleAfterDeactivate = employee.Role;

        employee.Activate();
        var roleAfterActivate = employee.Role;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(roleAfterAssignManager, Is.EqualTo(initialRole));
            Assert.That(roleAfterAssignPosition, Is.EqualTo(initialRole));
            Assert.That(roleAfterGrantAdmin, Is.EqualTo(initialRole));
            Assert.That(roleAfterRevokeAdmin, Is.EqualTo(initialRole));
            Assert.That(roleAfterDeactivate, Is.EqualTo(initialRole));
            Assert.That(roleAfterActivate, Is.EqualTo(initialRole));
        });
    }

    [Test]
    public void Resolve_CalledRepeatedlyWithSameInput_IsIdempotent()
    {
        // Arrange
        var employee = _employee;
        int activeSubordinateCount = 1;

        // Act
        var firstResult = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: activeSubordinateCount
        );

        var roleAfterFirstResolve = employee.Role;

        var secondResult = _roleResolver.Resolve(
            employee: employee,
            activeSubordinateCount: activeSubordinateCount
        );

        var roleAfterSecondResolve = employee.Role;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(firstResult, Is.EqualTo(SystemRole.SuperEmployee));
            Assert.That(secondResult, Is.EqualTo(firstResult));
            Assert.That(roleAfterFirstResolve, Is.EqualTo(SystemRole.SuperEmployee));
            Assert.That(roleAfterSecondResolve, Is.EqualTo(roleAfterFirstResolve));
        });
    }
}
