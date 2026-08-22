using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.ValueObjects;

[TestFixture]
public class PositionIdTests
{
    [Test]
    public void New_WhenCreatingPositionId_ReturnsNonEmpty()
    {
        var positionId = PositionId.New();

        Assert.That(positionId.Value, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void New_WhenCreatingTwoPositionId_ReturnsDifferentPositionId()
    {
        var firstPositionId = PositionId.New();
        var secondPositionId = PositionId.New();

        Assert.That(firstPositionId, Is.Not.EqualTo(secondPositionId));
    }

    [Test]
    public void Create_WithGuidValid_ReturnsSuccessfulResultWithSameGuid()
    {
        var guid = Guid.CreateVersion7();
        var result = PositionId.Create(guid);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Value.Value, Is.EqualTo(guid));
        });
    }

    [Test]
    public void Create_WithGuidEmpty_ReturnsFailedResult()
    {
        var guid = Guid.Empty;
        var result = PositionId.Create(guid);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Position ID cannot be empty"));
        });
    }

    [Test]
    public void Create_WithSameGuid_ReturnsEqualPositionIds()
    {
        var guid = Guid.CreateVersion7();

        var firstResult = PositionId.Create(guid);
        var secondResult = PositionId.Create(guid);

        Assert.That(secondResult.Value, Is.EqualTo(firstResult.Value));
    }

    [Test]
    public void ToString_WhenCalled_ReturnsValueAsString()
    {
        var guid = Guid.CreateVersion7();
        var positionId = PositionId.Create(guid).Value;

        Assert.That(positionId.ToString(), Is.EqualTo(guid.ToString()));
    }
}