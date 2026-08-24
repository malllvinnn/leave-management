using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class EmployeeTests
{
    [Test]
    public void Create_WithValidValues_ReturnsSuccessfulResultWithInitialState()
    {
        // Arrange
        var fullName = "John Doe";
        var email = Email.Create("johndoe@example.com").Value;
        var hireDate = new DateOnly(2026, 1, 1);
        var managerId = EmployeeId.New();
        var positionId = PositionId.New();

        // Act
        var result = Employee.Create(
            fullName,
            email,
            hireDate,
            managerId,
            positionId
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.Id.Value, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Value.FullName, Is.EqualTo(fullName));
            Assert.That(result.Value.Email, Is.EqualTo(email));
            Assert.That(result.Value.HireDate, Is.EqualTo(hireDate));
            Assert.That(result.Value.ManagerId, Is.EqualTo(managerId));
            Assert.That(result.Value.PositionId, Is.EqualTo(positionId));

            Assert.That(result.Value.AdminGrantedAt, Is.Null);
            Assert.That(result.Value.Role, Is.EqualTo(SystemRole.Employee));
            Assert.That(result.Value.IsActive, Is.True);
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("\t")]
    public void Create_WithNullEmptyOrWhitespaceFullName_ReturnsFailedResult(string? fullName)
    {
        // Act
        var result = Employee.Create(
            fullName,
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Employee full name cannot be empty"));
        });
    }

    [Test]
    public void Create_WithSurroundingWhitespaceInFullName_ReturnsSuccessfulResultWithTrimmedFullName()
    {
        // Arrange
        var fullName = "     John Doe  ";

        // Act
        var result = Employee.Create(
            fullName,
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.FullName, Is.EqualTo("John Doe"));
        });
    }

    [Test]
    public void Create_With200CharacterFullName_ReturnsSuccessfulResult()
    {
        // Arrange
        var fullName = new string('a', 200);

        // Act
        var result = Employee.Create(
            fullName,
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.FullName, Is.EqualTo(fullName));
            Assert.That(result.Value.FullName.Length, Is.EqualTo(200));
        });
    }

    [Test]
    public void Create_With201CharacterFullName_ReturnsFailedResult()
    {
        // Arrange
        var fullName = new string('a', 201);

        // Act
        var result = Employee.Create(
            fullName,
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Employee full name cannot exceed 200 characters"));
        });
    }

    [Test]
    public void Create_WithNullEmail_ReturnsFailedResult()
    {
        // Arrange
        Email? email = null;

        // Act
        var result = Employee.Create(
            fullName: "John Doe",
            email,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Employee email cannot be empty"));
        });
    }

    [Test]
    public void Create_WithFutureHireDate_ReturnsSuccessfulResultWithUnchangedHireDate()
    {
        // Arrange
        var email = Email.Create("johndoe@example.com").Value;
        var futureHireDate = new DateOnly(2030, 1, 1);

        // Act
        var result = Employee.Create(
            fullName: "John Doe",
            email,
            futureHireDate,
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.HireDate, Is.EqualTo(futureHireDate));
        });
    }

    [Test]
    public void Create_WithNullManagerIdAndPositionId_ReturnsSuccessfulResult()
    {
        // Arrange
        EmployeeId? managerId = null;
        PositionId? positionId = null;

        // Act
        var result = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId,
            positionId
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(result.Value.ManagerId, Is.Null);
            Assert.That(result.Value.PositionId, Is.Null);
        });
    }

    [Test]
    public void AssignManager_WithValidManagerId_ReturnsSuccessfulResult()
    {
        // Arrange
        var newManagerId = EmployeeId.New();

        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        var originalRole = employee.Role;

        // Act
        var result = employee.AssignManager(newManagerId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(employee.ManagerId, Is.EqualTo(newManagerId));
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void AssignManager_WithOwnEmployeeId_ReturnsFailedResultWithoutChangingManagerId()
    {
        // Arrange
        var originalManagerId = EmployeeId.New();

        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: originalManagerId,
            positionId: PositionId.New()
        ).Value;

        var ownEmployeeId = employee.Id;
        var originalRole = employee.Role;

        // Act
        var result = employee.AssignManager(ownEmployeeId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Employee cannot be assigned as their own manager"));

            Assert.That(employee.ManagerId, Is.EqualTo(originalManagerId));
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void AssignManager_WithNullManagerId_ReturnsSuccessfulResultWithClearedManagerId()
    {
        // Arrange
        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        var originalRole = employee.Role;

        // Act
        var result = employee.AssignManager(null);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(employee.ManagerId, Is.Null);
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void AssignPosition_WithValidPositionId_ReturnsSuccessfulResultWithoutChangingRole()
    {
        // Arrange
        var newPositionId = PositionId.New();

        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        var originalRole = employee.Role;

        // Act
        var result = employee.AssignPosition(newPositionId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(employee.PositionId, Is.EqualTo(newPositionId));
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void AssignPosition_WithNullPositionId_ReturnsSuccessfulResultWithClearedPositionId()
    {
        // Arrange
        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        var originalRole = employee.Role;

        // Act
        var result = employee.AssignPosition(null);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(employee.PositionId, Is.Null);
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void GrantAdmin_WithDifferentActorAndNonUtcTimestamp_ReturnsSuccessfulResultWithUtcTimestampPreservingInstant()
    {
        // Arrange
        var actorId = EmployeeId.New();

        var requestedGrantedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        var expectedUtcGrantedAt = requestedGrantedAt.ToUniversalTime();

        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        var originalRole = employee.Role;

        // Act
        var result = employee.GrantAdmin(actorId, requestedGrantedAt);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(employee.AdminGrantedAt, Is.EqualTo(expectedUtcGrantedAt));
            Assert.That(employee.AdminGrantedAt!.Value.Offset, Is.EqualTo(TimeSpan.Zero));
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void GrantAdmin_WithOwnEmployeeId_ReturnsFailedResultWithoutChangingAdminGrantedAt()
    {
        // Arrange
        var requestedGrantedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        var ownEmployeeId = employee.Id;
        var originalAdminGrantedAt = employee.AdminGrantedAt;
        var originalRole = employee.Role;

        // Act
        var result = employee.GrantAdmin(ownEmployeeId, requestedGrantedAt);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Employee cannot grant administrator access to themselves"));

            Assert.That(employee.AdminGrantedAt, Is.EqualTo(originalAdminGrantedAt));
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void GrantAdmin_WhenAlreadyGranted_ReturnsFailedResultWithoutChangingOriginalAdminGrantedAt()
    {
        // Arrange
        var actorId = EmployeeId.New();

        var initialGrantedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        var replacementGrantedAt = new DateTimeOffset(
            year: 2026,
            month: 11,
            day: 19,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        employee.GrantAdmin(actorId, initialGrantedAt);

        var originalAdminGrantedAt = employee.AdminGrantedAt;
        var originalRole = employee.Role;

        // Act
        var result = employee.GrantAdmin(actorId, replacementGrantedAt);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Administrator access has already been granted to the employee"));

            Assert.That(employee.AdminGrantedAt, Is.EqualTo(originalAdminGrantedAt));
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void RevokeAdmin_WithDifferentActorWhenGranted_ReturnsSuccessfulResultWithClearedAdminGrantedAt()
    {
        // Arrange
        var grantingActorId = EmployeeId.New();
        var revokingActorId = EmployeeId.New();

        var initialGrantedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        employee.GrantAdmin(grantingActorId, initialGrantedAt);

        var originalRole = employee.Role;

        // Act
        var result = employee.RevokeAdmin(revokingActorId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(employee.AdminGrantedAt, Is.Null);
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void RevokeAdmin_WithOwnEmployeeId_ReturnsFailedResultWithoutChangingAdminGrantedAt()
    {
        // Arrange
        var grantingActorId = EmployeeId.New();

        var initialGrantedAt = new DateTimeOffset(
            year: 2026,
            month: 8,
            day: 24,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        employee.GrantAdmin(grantingActorId, initialGrantedAt);

        var ownEmployeeId = employee.Id;
        var originalAdminGrantedAt = employee.AdminGrantedAt;
        var originalRole = employee.Role;

        // Act
        var result = employee.RevokeAdmin(ownEmployeeId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Employee cannot revoke their own administrator access"));

            Assert.That(employee.AdminGrantedAt, Is.EqualTo(originalAdminGrantedAt));
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void RevokeAdmin_WhenNotGranted_ReturnsFailedResult()
    {
        // Arrange
        var actorId = EmployeeId.New();

        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        var originalRole = employee.Role;

        // Act
        var result = employee.RevokeAdmin(actorId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Administrator access has not been granted to the employee"));

            Assert.That(employee.AdminGrantedAt, Is.Null);
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void Deactivate_WhenActive_ReturnsSuccessfulResultWithInactiveEmployee()
    {
        // Arrange
        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        var originalRole = employee.Role;

        // Act
        var result = employee.Deactivate();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(employee.IsActive, Is.False);
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void Deactivate_WhenAlreadyInactive_ReturnsFailedResult()
    {
        // Arrange
        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        employee.Deactivate();

        var originalRole = employee.Role;

        // Act
        var result = employee.Deactivate();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Employee is already inactive"));

            Assert.That(employee.IsActive, Is.False);
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void Activate_WhenInactive_ReturnsSuccessfulResultWithActiveEmployee()
    {
        // Arrange
        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        employee.Deactivate();

        var originalRole = employee.Role;

        // Act
        var result = employee.Activate();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(employee.IsActive, Is.True);
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }

    [Test]
    public void Activate_WhenAlreadyActive_ReturnsFailedResult()
    {
        // Arrange
        var employee = Employee.Create(
            fullName: "John Doe",
            email: Email.Create("johndoe@example.com").Value,
            hireDate: new DateOnly(2026, 1, 1),
            managerId: EmployeeId.New(),
            positionId: PositionId.New()
        ).Value;

        var originalRole = employee.Role;

        // Act
        var result = employee.Activate();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Employee is already active"));

            Assert.That(employee.IsActive, Is.True);
            Assert.That(employee.Role, Is.EqualTo(originalRole));
        });
    }
}
