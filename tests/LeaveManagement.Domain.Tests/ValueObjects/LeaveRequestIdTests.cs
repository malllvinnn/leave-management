using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.ValueObjects;

[TestFixture]
public class LeaveRequestIdTests
{
    [Test]
    public void New_WhenCreatingLeaveRequestId_ReturnsNonEmpty()
    {
        var leaveRequestId = LeaveRequestId.New();

        Assert.That(leaveRequestId.Value, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void New_WhenCreatingTwoLeaveRequestId_ReturnsDifferentLeaveRequestId()
    {
        var firstLeaveRequestId = LeaveRequestId.New();
        var secondLeaveRequestId = LeaveRequestId.New();

        Assert.That(firstLeaveRequestId, Is.Not.EqualTo(secondLeaveRequestId));
    }

    [Test]
    public void Create_WithGuidValid_ReturnsSuccessfulResultWithSameGuid()
    {
        var guid = Guid.CreateVersion7();
        var result = LeaveRequestId.Create(guid);

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
        var result = LeaveRequestId.Create(guid);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave Request ID cannot be empty"));
        });
    }

    [Test]
    public void Create_WithSameGuid_ReturnsEqualLeaveRequestIds()
    {
        var guid = Guid.CreateVersion7();

        var firstResult = LeaveRequestId.Create(guid);
        var secondResult = LeaveRequestId.Create(guid);

        Assert.That(secondResult.Value, Is.EqualTo(firstResult.Value));
    }

    [Test]
    public void ToString_WhenCalled_ReturnsValueAsString()
    {
        var guid = Guid.CreateVersion7();
        var leaveRequestId = LeaveRequestId.Create(guid).Value;

        Assert.That(leaveRequestId.ToString(), Is.EqualTo(guid.ToString()));
    }
}