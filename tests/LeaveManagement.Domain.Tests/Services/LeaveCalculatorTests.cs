using LeaveManagement.Domain.Services;

namespace LeaveManagement.Domain.Tests.Services;

[TestFixture]
public class LeaveCalculatorTests
{
    private LeaveCalculator _leaveCalculator;

    [SetUp]
    public void Setup()
    {
        _leaveCalculator = new LeaveCalculator();
    }
}