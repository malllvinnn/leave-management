using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.Tests.Common;

[TestFixture]
public class ResultTests
{
  [Test]
  public void Ok_WhenCalled_ReturnsSuccessfulResult()
  {
    var result = Result.Ok();

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Error, Is.Empty);
    });
  }

  [Test]
  public void Fail_WithValidError_ReturnsFailedResultWithError()
  {
    var result = Result.Fail("Something went wrong.");

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Something went wrong."));
    });
  }

  [Test]
  public void Fail_WithNullError_ThrowsArgumentNullException()
  {
    Assert.Throws<ArgumentNullException>(() => Result.Fail(null!));
  }

  [TestCase("")]
  [TestCase(" ")]
  [TestCase("\t")]
  public void Fail_WithEmptyOrWhitespaceError_ThrowsArgumentException(string error)
  {
    Assert.Throws<ArgumentException>(() => Result.Fail(error));
  }

  [Test]
  public void GenericOk_WithValue_ReturnsSuccessfulResultContainingValue()
  {
    var result = Result<int>.Ok(42);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Error, Is.Empty);
      Assert.That(result.Value, Is.EqualTo(42));
    });
  }

  [Test]
  public void GenericFail_WithValidError_ReturnsFailedResultWithError()
  {
    var result = Result<int>.Fail("Value is unavailable.");

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Value is unavailable."));
    });
  }

  [Test]
  public void Value_WhenResultIsFailure_ThrowsInvalidOperationException()
  {
    var result = Result<int>.Fail("Value is unavailable.");

    InvalidOperationException? exception = Assert.Throws<InvalidOperationException>(() => _ = result.Value);

    Assert.That(exception!.Message, Is.EqualTo("Value is unavailable."));
  }

  [Test]
  public void GenericFail_WithNullError_ThrowsArgumentNullException()
  {
    Assert.Throws<ArgumentNullException>(() => Result<int>.Fail(null!));
  }

  [TestCase("")]
  [TestCase(" ")]
  [TestCase("\t")]
  public void GenericFail_WithEmptyOrWhitespaceError_ThrowsArgumentException(
    string error)
  {
    Assert.Throws<ArgumentException>(() => Result<int>.Fail(error));
  }
}