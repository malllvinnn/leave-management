using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.ValueObjects;

[TestFixture]
public class EmployeeIdTests
{
  [Test]
  public void New_WhenCreatingEmployeeId_ReturnsNonEmpty()
  {
    var employeeId = EmployeeId.New();

    Assert.That(employeeId.Value, Is.Not.EqualTo(Guid.Empty));
  }

  [Test]
  public void New_WhenCreatingTwoEmployeeId_ReturnsDifferentEmployeeId()
  {
    var firstEmployeeId = EmployeeId.New();
    var secondEmployeeId = EmployeeId.New();

    Assert.That(firstEmployeeId, Is.Not.EqualTo(secondEmployeeId));
  }

  [Test]
  public void Create_WithGuidValid_ReturnsSuccessfulResultWithSameGuid()
  {
    var guid = Guid.CreateVersion7();
    var result = EmployeeId.Create(guid);

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
    var result = EmployeeId.Create(guid);

    Assert.Multiple(() =>
    {
      Assert.That(result.IsSuccess, Is.False);
      Assert.That(result.IsFailure, Is.True);
      Assert.That(result.Error, Is.EqualTo("Employee ID cannot be empty"));
    });
  }

  [Test]
  public void Create_WithSameGuid_ReturnsEqualEmployeeIds()
  {
    var guid = Guid.CreateVersion7();

    var firstResult = EmployeeId.Create(guid);
    var secondResult = EmployeeId.Create(guid);

    Assert.That(secondResult.Value, Is.EqualTo(firstResult.Value));
  }

  [Test]
  public void ToString_WhenCalled_ReturnsValueAsString()
  {
    var guid = Guid.CreateVersion7();
    var employeeId = EmployeeId.Create(guid).Value;

    Assert.That(employeeId.ToString(), Is.EqualTo(guid.ToString()));
  }
}