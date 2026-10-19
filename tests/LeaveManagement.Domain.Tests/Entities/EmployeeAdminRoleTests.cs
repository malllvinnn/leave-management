using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
[TestFixture]
public class EmployeeAdminRoleTests : EmployeeTestBase
{

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
        var employee = _validEmployee;
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
        var employee = _validEmployee;
        var ownEmployeeId = employee.Id;
        var originalAdminGrantedAt = employee.AdminGrantedAt;
        var originalRole = employee.Role;

        // Act
        var result = employee.GrantAdmin(ownEmployeeId, _initialGrantedAt);

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

        var replacementGrantedAt = new DateTimeOffset(
            year: 2026,
            month: 11,
            day: 19,
            hour: 10,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        var employee = _validEmployee;

        employee.GrantAdmin(actorId, _initialGrantedAt);

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
        var employee = _validEmployee;

        employee.GrantAdmin(grantingActorId, _initialGrantedAt);

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
        var employee = _validEmployee;

        employee.GrantAdmin(grantingActorId, _initialGrantedAt);

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
        var employee = _validEmployee;
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
}
