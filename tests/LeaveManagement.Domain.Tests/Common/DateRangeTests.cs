using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.Tests.Common;

[TestFixture]
public class DateRangeTests
{
  [Test]
  public void Create_WhenEndIsBeforeStart_ReturnsFailedResult()
  {
    var startDate = new DateOnly(2026, 8, 12);
    var endDate = new DateOnly(2026, 8, 9);

    var result = DateRange.Create(startDate, endDate);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("End date cannot be earlier than start date"));
    });
  }

  [Test]
  public void Create_WhenStartEqualsEnd_ReturnsSuccessfulResult()
  {
    var startDate = new DateOnly(2026, 8, 12);
    var endDate = new DateOnly(2026, 8, 12);

    var result = DateRange.Create(startDate, endDate);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.True);
      Assert.That(result.IsFailure, Is.False);
      Assert.That(result.Value.Start, Is.EqualTo(startDate));
      Assert.That(result.Value.End, Is.EqualTo(endDate));
    });
  }

  [Test]
  public void OverlapsWith_WhenFirstRangeIsEntirelyBeforeSecondRange_ReturnsFalse()
  {
    var firstRange = DateRange.Create(
        new DateOnly(2026, 8, 1),
        new DateOnly(2026, 8, 5));

    var secondRange = DateRange.Create(
        new DateOnly(2026, 8, 6),
        new DateOnly(2026, 8, 10));

    var overlaps = firstRange.Value.OverlapsWith(secondRange.Value);

    Assert.That(overlaps, Is.False);
  }

  [Test]
  public void OverlapsWith_WhenFirstRangeIsEntirelyAfterSecondRange_ReturnsFalse()
  {
    var firstRange = DateRange.Create(
        new DateOnly(2026, 8, 6),
        new DateOnly(2026, 8, 10));

    var secondRange = DateRange.Create(
        new DateOnly(2026, 8, 1),
        new DateOnly(2026, 8, 5));

    var overlaps = firstRange.Value.OverlapsWith(secondRange.Value);

    Assert.That(overlaps, Is.False);
  }

  [Test]
  public void OverlapsWith_WhenRangesTouchAtBoundary_ReturnsTrue()
  {
    var firstRange = DateRange.Create(
        new DateOnly(2026, 8, 1),
        new DateOnly(2026, 8, 5));

    var secondRange = DateRange.Create(
        new DateOnly(2026, 8, 5),
        new DateOnly(2026, 8, 10));

    var overlaps = firstRange.Value.OverlapsWith(secondRange.Value);

    Assert.That(overlaps, Is.True);
  }

  [Test]
  public void OverlapsWith_WhenSecondRangeIsContainedWithinFirstRange_ReturnsTrue()
  {
    var firstRange = DateRange.Create(
        new DateOnly(2026, 8, 1),
        new DateOnly(2026, 8, 10));

    var secondRange = DateRange.Create(
        new DateOnly(2026, 8, 3),
        new DateOnly(2026, 8, 7));

    var overlaps = firstRange.Value.OverlapsWith(secondRange.Value);

    Assert.That(overlaps, Is.True);
  }

  [Test]
  public void OverlapsWith_WhenRangesAreIdentical_ReturnsTrue()
  {
    var firstRange = DateRange.Create(
        new DateOnly(2026, 8, 1),
        new DateOnly(2026, 8, 10));

    var secondRange = DateRange.Create(
        new DateOnly(2026, 8, 1),
        new DateOnly(2026, 8, 10));

    var overlaps = firstRange.Value.OverlapsWith(secondRange.Value);

    Assert.That(overlaps, Is.True);
  }

  [Test]
  public void DurationInDays_WhenRangeIsSingleDay_ReturnsOne()
  {
    var date = new DateOnly(2026, 8, 12);
    var dateRangeResult = DateRange.Create(date, date);

    var duration = dateRangeResult.Value.DurationInDays();

    Assert.That(duration, Is.EqualTo(1));
  }

  [Test]
  public void DurationInDays_WhenRangeSpansMultipleDays_ReturnsInclusiveDuration()
  {
    var startDate = new DateOnly(2026, 8, 12);
    var endDate = new DateOnly(2026, 8, 15);
    var dateRangeResult = DateRange.Create(startDate, endDate);

    var duration = dateRangeResult.Value.DurationInDays();

    Assert.That(duration, Is.EqualTo(4));
  }
}
