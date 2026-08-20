using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.ValueObjects;

[TestFixture]
public class EmailTests
{
  [Test]
  public void Create_WhenEmailValid_ReturnsSuccessfulResult()
  {
    var result = Email.Create("johndoe@example.com");

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Value.Value, Is.EqualTo("johndoe@example.com"));
    });
  }

  [TestCase(null)]
  [TestCase("")]
  [TestCase(" ")]
  [TestCase("\t")]
  public void Create_WhenEmailIsNullOrWhiteSpace_ReturnsFailedResult(string? value)
  {
    var result = Email.Create(value);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Email cannot be empty"));
    });
  }

  [Test]
  public void Create_WhenEmailWithoutAtSign_ReturnsFailedResult()
  {
    var result = Email.Create("budix.com");

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Email format is invalid"));
    });
  }

  [TestCase("budi@")]
  [TestCase("@x.com")]
  [TestCase("budi@@x.com")]
  public void Create_WhenEmailFormatInvalid_ReturnsFailedResult(string value)
  {
    var result = Email.Create(value);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Email format is invalid"));
    });
  }

  [Test]
  public void Create_WithEmailUppercaseValue_ReturnsSuccessfulResult()
  {
    var result = Email.Create("BUDI@X.com");

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Value.Value, Is.EqualTo("budi@x.com"));
    });
  }

  [Test]
  public void Create_WithEmailUppercaseValueToEqualLowercaseValue_ReturnsSuccessfulResult()
  {
    var uppercaseEmail = Email.Create("BUDI@X.com").Value;
    var lowercaseEmail = Email.Create("budi@x.com").Value;
    var result = lowercaseEmail.Equals(uppercaseEmail);

    Assert.That(result, Is.True);
  }

  [Test]
  public void Create_WhenEmailExceedsMaximumLength_ReturnsFailedResult()
  {
    var localPart = new string('a', 250);
    var emailValue = $"{localPart}@x.com"; // a..(250) + @x.com (6) = 256

    var result = Email.Create(emailValue);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(emailValue, Has.Length.EqualTo(256));
      Assert.That(result.Error, Is.EqualTo("Email cannot exceed 255 characters"));
    });
  }

  [Test]
  public void Create_WhenEmailContainsDisplayName_ReturnsFailedResult()
  {
    var result = Email.Create("Budi <budi@x.com>");

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Email format is invalid"));
    });
  }

  [Test]
  public void Create_WhenEmailContainsWhitespace_ReturnsNormalizedValueAndSuccess()
  {
    var result = Email.Create("  BUDI@X.com  ");

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Value.Value, Is.EqualTo("budi@x.com"));
    });
  }
}