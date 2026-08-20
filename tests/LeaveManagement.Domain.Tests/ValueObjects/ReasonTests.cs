using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.ValueObjects;

[TestFixture]
public class ReasonTests
{
  [Test]
  public void Create_WithReasonValid_ReturnsSuccessfulResult()
  {
    var result = Reason.Create("Attending a family event outside the city");

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Value.Value, Is.EqualTo("Attending a family event outside the city"));
    });
  }

  [Test]
  public void Create_WhenReasonContainsCapitalizationAndPunctuation_PreservesOriginalText()
  {
    var reasonValue = "Family Event: Attending my sister's wedding in Semarang";

    var result = Reason.Create(reasonValue);

    Assert.That(result.Value.Value, Is.EqualTo(reasonValue));
  }

  [TestCase(null)]
  [TestCase("")]
  [TestCase(" ")]
  [TestCase("\t")]
  public void Create_WhenReasonIsNullOrWhiteSpace_ReturnsFailedResult(string? value)
  {
    var result = Reason.Create(value);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Reason cannot be empty"));
    });
  }

  [Test]
  public void Create_WhenReasonWithSpaceInStartOrEnd_ReturnsNormalizedValueAndSuccessfulResult()
  {
    var result = Reason.Create("     Attending a family event outside the city         ");

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Value.Value, Is.EqualTo("Attending a family event outside the city"));
    });
  }

  [Test]
  public void Create_WhenReasonWith500CharAndSpace_ReturnsSuccessfulResult()
  {
    var localPart = new string('a', 500);
    var reasonValue = $"          {localPart}"; // 10 space + 500 char... should be 510
    var result = Reason.Create(reasonValue); // Trimming leaves exactly 500 char

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Value.Value.Length, Is.EqualTo(500));
    });
  }

  [Test]
  public void Create_WhenReasonWithOverMaximumLength500_ReturnsFailedResult()
  {
    var localPart = new string('a', 500);
    var reasonValue = $"{localPart} Hello World"; // 500 char + Hello World... should be over value
    var result = Reason.Create(reasonValue);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Reason cannot exceed 500 characters"));
    });
  }

  [Test]
  public void Create_WhenReasonsHaveSameNormalizedValue_ReturnsTrue()
  {
    var reasonWithWhitespace = Reason.Create("  Attending a family event outside the city  ").Value;
    var normalizedReason = Reason.Create("Attending a family event outside the city").Value;
    var result = reasonWithWhitespace.Equals(normalizedReason);

    Assert.That(result, Is.True);
  }
}