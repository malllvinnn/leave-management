using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class AuditEntryCreateTests : AuditEntryTestBase
{
    [Test]
    public void Create_WithValidData_ReturnsEntryStoringProvidedValues()
    {
        // Arrange
        var actorId = EmployeeId.New();
        var entityType = "LeaveRequest";
        var entityId = "3f2504e0-4f89-11d3-9a0c-0305e82c3301";
        var action = "Approved";
        var payload = "{\"status\":\"Approved\"}";
        var actorSnapshot = "Sari — Backend Developer — Employee";

        var occurredAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 4,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        // Act
        var result = CreateEntry(
            entityType: entityType,
            entityId: entityId,
            action: action,
            payload: payload,
            actorSnapshot: actorSnapshot,
            actorId: actorId,
            occurredAt: occurredAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(entry.ActorId, Is.EqualTo(actorId));
            Assert.That(entry.EntityType, Is.EqualTo(entityType));
            Assert.That(entry.EntityId, Is.EqualTo(entityId));
            Assert.That(entry.Action, Is.EqualTo(action));
            Assert.That(entry.Payload, Is.EqualTo(payload));
            Assert.That(entry.ActorSnapshot, Is.EqualTo(actorSnapshot));
            Assert.That(entry.OccurredAt, Is.EqualTo(occurredAt.ToUniversalTime()));
        });
    }

    [Test]
    public void Create_WithSystemActor_ReturnsEntryWithNullActorIdAndSnapshot()
    {
        // Act
        var result = CreateEntry(
            actorId: null,
            actorSnapshot: null
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.ActorId, Is.Null);
            Assert.That(entry.ActorSnapshot, Is.Null);
        });
    }

    [Test]
    public void Create_WithSurroundingWhitespace_TrimsActorSnapshot()
    {
        // Arrange
        var actorSnapshot = " Sari — Backend Developer — Employee ";

        // Act
        var result = CreateEntry(
            actorSnapshot: actorSnapshot
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.ActorSnapshot, Is.EqualTo("Sari — Backend Developer — Employee"));
        });
    }

    [Test]
    public void Create_WithSurroundingWhitespace_TrimsEntityType()
    {
        // Arrange
        var entityType = " LeaveRequest ";

        // Act
        var result = CreateEntry(
            entityType: entityType
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.EntityType, Is.EqualTo("LeaveRequest"));
        });
    }

    [Test]
    public void Create_WithSurroundingWhitespace_TrimsEntityId()
    {
        // Arrange
        var entityId = " 3f2504e0-4f89-11d3-9a0c-0305e82c3301 ";

        // Act
        var result = CreateEntry(
            entityId: entityId
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.EntityId, Is.EqualTo("3f2504e0-4f89-11d3-9a0c-0305e82c3301"));
        });
    }

    [Test]
    public void Create_WithSurroundingWhitespace_TrimsAction()
    {
        // Arrange
        var action = "       Approved     ";

        // Act
        var result = CreateEntry(
            action: action
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.Action, Is.EqualTo("Approved"));
        });
    }

    [Test]
    public void Create_WithSurroundingWhitespaceInPayload_PreservesPayload()
    {
        // Arrange
        var payload = " {\"status\": \"Approved\"} ";

        // Act
        var result = CreateEntry(
            payload: payload
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.Payload, Is.EqualTo(payload));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_WithNullEmptyOrWhitespaceEntityType_ReturnsFailedResult(string? entityType)
    {
        // Act
        var result = CreateEntry(
            entityType: entityType
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Entity type cannot be empty"));
        });
    }

    [Test]
    public void Create_With100CharacterEntityType_ReturnsSuccessfulResult()
    {
        // Arrange
        var entityType = new string('a', 100);

        // Act
        var result = CreateEntry(
            entityType: entityType
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.EntityType, Has.Length.EqualTo(100));
        });
    }

    [Test]
    public void Create_With101CharacterEntityType_ReturnsFailedResult()
    {
        // Arrange
        var entityType = new string('a', 101);

        // Act
        var result = CreateEntry(
            entityType: entityType
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Entity type cannot exceed 100 characters"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_WithNullEmptyOrWhitespaceEntityId_ReturnsFailedResult(string? entityId)
    {
        // Act
        var result = CreateEntry(
            entityId: entityId
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Entity id cannot be empty"));
        });
    }

    [Test]
    public void Create_With100CharacterEntityId_ReturnsSuccessfulResult()
    {
        // Arrange
        var entityId = new string('a', 100);

        // Act
        var result = CreateEntry(
            entityId: entityId
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.EntityId, Has.Length.EqualTo(100));
        });
    }

    [Test]
    public void Create_With101CharacterEntityId_ReturnsFailedResult()
    {
        // Arrange
        var entityId = new string('a', 101);

        // Act
        var result = CreateEntry(
            entityId: entityId
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Entity id cannot exceed 100 characters"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_WithNullEmptyOrWhitespaceAction_ReturnsFailedResult(string? action)
    {
        // Act
        var result = CreateEntry(
            action: action
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Action cannot be empty"));
        });
    }

    [Test]
    public void Create_With100CharacterAction_ReturnsSuccessfulResult()
    {
        // Arrange
        var action = new string('a', 100);

        // Act
        var result = CreateEntry(
            action: action
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.Action, Has.Length.EqualTo(100));
        });
    }

    [Test]
    public void Create_With101CharacterAction_ReturnsFailedResult()
    {
        // Arrange
        var action = new string('a', 101);

        // Act
        var result = CreateEntry(
            action: action
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Action cannot exceed 100 characters"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_WithNullEmptyOrWhitespacePayload_ReturnsFailedResult(string? payload)
    {
        // Act
        var result = CreateEntry(
            payload: payload
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Payload cannot be empty"));
        });
    }

    [Test]
    public void Create_With300CharacterActorSnapshot_ReturnsSuccessfulResult()
    {
        // Arrange
        var actorSnapshot = new string('a', 300);

        // Act
        var result = CreateEntry(
            actorSnapshot: actorSnapshot
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.ActorSnapshot, Has.Length.EqualTo(300));
        });
    }

    [Test]
    public void Create_With301CharacterActorSnapshot_ReturnsFailedResult()
    {
        // Arrange
        var actorSnapshot = new string('a', 301);

        // Act
        var result = CreateEntry(
            actorSnapshot: actorSnapshot
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Actor snapshot cannot exceed 300 characters"));
        });
    }

    [Test]
    public void Create_WithNonUtcTimestamp_StoresUtcPreservingInstant()
    {
        // Arrange
        var occurredAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 4,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        // Act
        var result = CreateEntry(
            occurredAt: occurredAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var entry = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(entry.OccurredAt, Is.EqualTo(occurredAt.ToUniversalTime()));
            Assert.That(entry.OccurredAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }
}