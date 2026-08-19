using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.Tests.Common;

[TestFixture]
public class LeaveDaysTests
{
  [Test]
  public void Create_WithPositiveValue_ReturnsSuccessfulResult()
  {
    var result = LeaveDays.Create(5);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Error, Is.Empty);
      Assert.That(result.Value.Value, Is.EqualTo(5));
    });
  }

  [Test]
  public void Create_WithNegativeValue_ReturnsFailedResultWithError()
  {
    var result = LeaveDays.Create(-1);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Leave days cannot be negative"));
    });
  }

  [Test]
  public void Create_WithZeroValue_ReturnsSuccessfulResult()
  {
    var result = LeaveDays.Create(0);
    var zero = LeaveDays.Zero;

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Error, Is.Empty);
      Assert.That(result.Value.Value, Is.EqualTo(0));
      Assert.That(result.Value, Is.EqualTo(zero));
    });
  }

  [Test]
  public void Add_WithTwoLeaveDays_ReturnsCombinedValue()
  {
    var twoDaysResult = LeaveDays.Create(2);
    var threeDaysResult = LeaveDays.Create(3);
    var result = twoDaysResult.Value.Add(threeDaysResult.Value);

    Assert.That(result.Value, Is.EqualTo(5));
  }

  [Test]
  public void Subtract_WhenResultWouldBeNegative_ReturnsFailedResultWithError()
  {
    var fiveDaysResult = LeaveDays.Create(5).Value;
    var overDays = LeaveDays.Create(10).Value;
    var result = fiveDaysResult.Subtract(overDays);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Leave days cannot be negative"));
    });
  }

  [Test]
  public void Subtract_WhenResultIsZero_ReturnsSuccessfulResult()
  {
    var fiveDaysResult = LeaveDays.Create(5).Value;
    var result = fiveDaysResult.Subtract(fiveDaysResult);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Error, Is.Empty);
      Assert.That(result.Value, Is.EqualTo(LeaveDays.Zero));
    });
  }

  [Test]
  public void ToString_WhenCalled_ReturnsFormattedLeaveDays()
  {
    var leaveDays = LeaveDays.Create(5).Value;
    var result = leaveDays.ToString();

    Assert.That(result, Is.EqualTo("5 days"));
  }
}
