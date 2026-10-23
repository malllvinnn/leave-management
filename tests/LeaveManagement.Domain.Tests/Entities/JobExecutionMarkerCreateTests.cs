using LeaveManagement.Domain.Entities;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
public class JobExecutionMarkerCreateTests : JobExecutionMarkerTestBase
{
    [Test]
    public void Create_WithValidData_ReturnsMarkerStoringProvidedValues()
    {
        // Arrange
        var jobName = "CarryOverExpiry";
        var periodKey = "2027";

        var executedAt = new DateTimeOffset(
            year: 2027,
            month: 1,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        // Act
        var result = CreateMarker(
            jobName: jobName,
            periodKey: periodKey,
            executedAt: executedAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var jobExecutionMarker = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(jobExecutionMarker.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(jobExecutionMarker.JobName, Is.EqualTo(jobName));
            Assert.That(jobExecutionMarker.PeriodKey, Is.EqualTo(periodKey));
            Assert.That(jobExecutionMarker.ExecutedAt, Is.EqualTo(executedAt.ToUniversalTime()));
        });
    }

    [Test]
    public void Create_WithSurroundingWhitespace_TrimsJobName()
    {
        // Arrange
        var jobName = " CarryOverExpiry ";

        var executedAt = new DateTimeOffset(
            year: 2027,
            month: 1,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        // Act
        var result = CreateMarker(
            jobName: jobName
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var jobExecutionMarker = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(jobExecutionMarker.JobName, Is.EqualTo("CarryOverExpiry"));
            Assert.That(jobExecutionMarker.ExecutedAt, Is.EqualTo(executedAt.ToUniversalTime()));
        });
    }

    [Test]
    public void Create_WithSurroundingWhitespace_TrimsPeriodKey()
    {
        // Arrange
        var periodKey = " 2027 ";

        // Act
        var result = CreateMarker(
            periodKey: periodKey
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var jobExecutionMarker = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(jobExecutionMarker.PeriodKey, Is.EqualTo("2027"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_WithNullEmptyOrWhitespaceJobName_ReturnsFailedResult(string? jobName)
    {
        // Act
        var result = CreateMarker(
            jobName: jobName
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Job name cannot be empty"));
        });
    }

    [Test]
    public void Create_With100CharacterJobName_ReturnsSuccessfulResult()
    {
        // Arrange
        var jobName = new string('a', 100);

        // Act
        var result = CreateMarker(
            jobName: jobName
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var jobExecutionMarker = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(jobExecutionMarker.JobName, Has.Length.EqualTo(100));
        });
    }

    [Test]
    public void Create_With101CharacterJobName_ReturnsFailedResult()
    {
        // Arrange
        var jobName = new string('a', 101);

        // Act
        var result = CreateMarker(
            jobName: jobName
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Job name cannot exceed 100 characters"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_WithNullEmptyOrWhitespacePeriodKey_ReturnsFailedResult(string? periodKey)
    {
        // Act
        var result = CreateMarker(
            periodKey: periodKey
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Period key cannot be empty"));
        });
    }

    [Test]
    public void Create_With50CharacterPeriodKey_ReturnsSuccessfulResult()
    {
        // Arrange
        var periodKey = new string('a', 50);

        // Act
        var result = CreateMarker(
            periodKey: periodKey
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var jobExecutionMarker = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(jobExecutionMarker.PeriodKey, Has.Length.EqualTo(50));
        });
    }

    [Test]
    public void Create_With51CharacterPeriodKey_ReturnsFailedResult()
    {
        // Arrange
        var periodKey = new string('a', 51);

        // Act
        var result = CreateMarker(
            periodKey: periodKey
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Period key cannot exceed 50 characters"));
        });
    }

    [Test]
    public void Create_WithNonUtcTimestamp_StoresUtcPreservingInstant()
    {
        // Arrange
        var executedAt = new DateTimeOffset(
            year: 2027,
            month: 1,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        // Act
        var result = CreateMarker(
            executedAt: executedAt
        );

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var jobExecutionMarker = result.Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Error, Is.Empty);

            Assert.That(jobExecutionMarker.ExecutedAt, Is.EqualTo(executedAt.ToUniversalTime()));
            Assert.That(jobExecutionMarker.ExecutedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        });
    }
}