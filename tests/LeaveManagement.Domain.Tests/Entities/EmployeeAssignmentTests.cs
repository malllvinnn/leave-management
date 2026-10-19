using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
[TestFixture]
public class EmployeeAssignmentTests : EmployeeTestBase
{

    [Test]
    public void AssignManager_WithValidManagerId_ReturnsSuccessfulResult()
    {
        // Arrange
        var newManagerId = EmployeeId.New();
        var employee = _validEmployee;
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
        var employee = _validEmployee;
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
        var employee = _validEmployee;
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
        var employee = _validEmployee;
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
}
