using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.ValueObjects;

[TestFixture]
public class LeaveBalanceIdTests
{
    [Test]
    public void New_WhenCreatingLeaveBalanceId_ReturnsNonEmpty()
    {
        var leaveBalanceId = LeaveBalanceId.New();

        Assert.That(leaveBalanceId.Value, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void New_WhenCreatingTwoLeaveBalanceId_ReturnsDifferentLeaveBalanceId()
    {
        var firstLeaveBalanceId = LeaveBalanceId.New();
        var secondLeaveBalanceId = LeaveBalanceId.New();

        Assert.That(firstLeaveBalanceId, Is.Not.EqualTo(secondLeaveBalanceId));
    }

    [Test]
    public void Create_WithGuidValid_ReturnsSuccessfulResultWithSameGuid()
    {
        var guid = Guid.CreateVersion7();
        var result = LeaveBalanceId.Create(guid);

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
        var result = LeaveBalanceId.Create(guid);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Leave Balance ID cannot be empty"));
        });
    }

    [Test]
    public void Create_WithSameGuid_ReturnsEqualLeaveBalanceIds()
    {
        var guid = Guid.CreateVersion7();

        var firstResult = LeaveBalanceId.Create(guid);
        var secondResult = LeaveBalanceId.Create(guid);

        Assert.That(secondResult.Value, Is.EqualTo(firstResult.Value));
    }

    [Test]
    public void ToString_WhenCalled_ReturnsValueAsString()
    {
        var guid = Guid.CreateVersion7();
        var leaveBalanceId = LeaveBalanceId.Create(guid).Value;

        Assert.That(leaveBalanceId.ToString(), Is.EqualTo(guid.ToString()));
    }
}