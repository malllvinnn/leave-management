using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class EmployeeCreateTests : EmployeeTestBase
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
}
