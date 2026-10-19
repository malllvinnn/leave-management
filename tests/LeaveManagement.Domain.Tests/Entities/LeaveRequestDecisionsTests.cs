using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

[TestFixture]
[TestFixture]
public class LeaveRequestDecisionsTests : LeaveRequestTestBase
{

    [Test]
    public void Decisions_WhenPublicMutationIsAttempted_PreservesDecisionHistory()
    {
        // Arrange
        var createResult = CreatePendingRequestForStage(ApprovalStage.First);

        Assert.That(createResult.IsSuccess, Is.True);

        var request = createResult.Value;

        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Pending));
        Assert.That(request.Decisions, Has.Count.EqualTo(0));

        var approveFirstStageResult = request.ApproveFirstStage(
            approverId: _approverId,
            requiresFinalApproval: false,
            reason: _validApprovalReason,
            now: _firstApprovalAt
        );

        Assert.That(approveFirstStageResult.IsSuccess, Is.True);
        Assert.That(request.Status, Is.EqualTo(LeaveStatus.Approved));
        Assert.That(request.Decisions, Has.Count.EqualTo(1));

        var firstDecisionId = request.Decisions[0].Id;

        // Act
        Assert.Throws<NotSupportedException>(() => ((IList<ApprovalDecision>)request.Decisions).Clear());

        // Assert
        Assert.That(request.Decisions, Has.Count.EqualTo(1));
        Assert.That(request.Decisions[0].Id, Is.EqualTo(firstDecisionId));
    }
}
