using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class EmployeeActivationTests : EmployeeTestBase
{

    [Test]
    public void Deactivate_WhenActive_ReturnsSuccessfulResultWithInactiveEmployee()
    {
        // Arrange
        var employee = _validEmployee;
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
        var employee = _validEmployee;

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
        var employee = _validEmployee;

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
        var employee = _validEmployee;
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
