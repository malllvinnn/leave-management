using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class HolidayCreateTests
{
    [TestCase(HolidaySource.Manual)]
    [TestCase(HolidaySource.Imported)]
    public void Create_WithValidData_ReturnsHolidayWithProvidedSource(
        HolidaySource source
    )
    {
        // Arrange
        var date = new DateOnly(2026, 1, 1);
        var name = "Hari Raya Idul Fitri";

        // Act
        var result = Holiday.Create(
            date: date,
            name: name,
            source: source
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var holiday = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(holiday.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(holiday.Date, Is.EqualTo(date));
            Assert.That(holiday.Name, Is.EqualTo(name));
            Assert.That(holiday.Source, Is.EqualTo(source));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_WithNullEmptyOrWhitespaceName_ReturnsFailedResult(string? name)
    {
        // Act
        var result = Holiday.Create(
            date: new DateOnly(2026, 1, 1),
            name: name,
            source: HolidaySource.Manual
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Holiday name cannot be empty"));
        });
    }

    [Test]
    public void Create_WithSurroundingWhitespace_TrimsName()
    {
        // Arrange
        var name = " Nyepi ";

        // Act
        var result = Holiday.Create(
            date: new DateOnly(2026, 3, 19),
            name: name,
            source: HolidaySource.Manual
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var holiday = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(holiday.Name, Is.EqualTo("Nyepi"));
        });
    }

    [Test]
    public void Create_With150CharacterName_ReturnsSuccessfulResult()
    {
        // Arrange
        var name = new string('a', 150);

        // Act
        var result = Holiday.Create(
            date: new DateOnly(2026, 1, 1),
            name: name,
            source: HolidaySource.Manual
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var holiday = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(holiday.Name.Length, Is.EqualTo(150));
            Assert.That(holiday.Name, Is.EqualTo(name));
        });
    }

    [Test]
    public void Create_With151CharacterName_ReturnsFailedResult()
    {
        // Arrange
        var name = new string('a', 151);

        // Act
        var result = Holiday.Create(
            date: new DateOnly(2026, 1, 1),
            name: name,
            source: HolidaySource.Manual
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Holiday name cannot exceed 150 characters"));
        });
    }
}