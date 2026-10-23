using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class PositionCreateTests
{
    [Test]
    public void Create_WithValidName_ReturnsActivePosition()
    {
        // Arrange
        var name = "Backend Developer";

        // Act
        var result = Position.Create(name);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var position = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(position.Id.Value, Is.Not.EqualTo(Guid.Empty));
            Assert.That(position.Name, Is.EqualTo(name));
            Assert.That(position.IsActive, Is.True);
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_WithNullEmptyOrWhitespaceName_ReturnsFailedResult(string? name)
    {
        // Act
        var result = Position.Create(name);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Position name cannot be empty"));
        });
    }

    [Test]
    public void Create_WithSurroundingWhitespace_TrimsName()
    {
        // Arrange
        var name = " HR Manager ";

        // Act
        var result = Position.Create(name);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var position = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(position.Name, Is.EqualTo("HR Manager"));
        });
    }

    [Test]
    public void Create_With150CharacterName_ReturnsSuccessfulResult()
    {
        // Arrange
        var name = new string('a', 150);

        // Act
        var result = Position.Create(name);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var position = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(position.Name.Length, Is.EqualTo(150));
            Assert.That(position.Name, Is.EqualTo(name));
        });
    }

    [Test]
    public void Create_With151CharacterName_ReturnsFailedResult()
    {
        // Arrange
        var name = new string('a', 151);

        // Act
        var result = Position.Create(name);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Position name cannot exceed 150 characters"));
        });
    }
}