using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Entities;

public class LeaveRequest
{
    public LeaveRequestId Id { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public DateRange Period { get; private set; }
    public LeaveDays WorkingDays { get; private set; }
    public LeaveAllocation Allocation { get; private set; }
    public LeaveType Type { get; private set; }
    public Reason Reason { get; private set; }
    public LeaveStatus Status { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }

    public IReadOnlyList<ApprovalDecision> Decisions
    {
        get
        {
            return _decisions.AsReadOnly();
        }
    }

    private readonly List<ApprovalDecision> _decisions = [];

    private LeaveRequest(
        LeaveRequestId id,
        EmployeeId employeeId,
        DateRange period,
        LeaveDays workingDays,
        LeaveAllocation allocation,
        LeaveType type,
        Reason reason,
        DateTimeOffset submittedAt
    )
    {
        Id = id;
        EmployeeId = employeeId;
        Period = period;
        WorkingDays = workingDays;
        Allocation = allocation;
        Type = type;
        Reason = reason;
        SubmittedAt = submittedAt.ToUniversalTime();

        Status = LeaveStatus.Pending;
    }

    public static Result<LeaveRequest> Create(
        EmployeeId employeeId,
        DateRange period,
        LeaveDays workingDays,
        LeaveAllocation allocation,
        LeaveType type,
        Reason reason,
        DateOnly today,
        DateTimeOffset submittedAt
    )
    {
        if (allocation.Total != workingDays)
        {
            var failureResult = Result<LeaveRequest>.Fail("Leave allocation must equal the working days of the request");

            return failureResult;
        }

        var newId = LeaveRequestId.New();

        var leaveRequest = new LeaveRequest(
            id: newId,
            employeeId: employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: type,
            reason: reason,
            submittedAt: submittedAt
        );

        var successResult = Result<LeaveRequest>.Ok(leaveRequest);

        return successResult;
    }

    public Result ApproveFirstStage(
        EmployeeId approverId,
        bool requiresFinalApproval,
        Reason? reason,
        DateTimeOffset now
    )
    {
        throw new NotImplementedException();
    }

    public Result ApproveFinalStage(
        EmployeeId approverId,
        Reason? reason,
        DateTimeOffset now
    )
    {
        throw new NotImplementedException();
    }

    public Result Override(
        EmployeeId actorId,
        EmployeeId? bypassedApproverId,
        DecisionType outcome,
        Reason? reason,
        DateTimeOffset now
    )
    {
        throw new NotImplementedException();
    }

    public Result Reject(
        EmployeeId actorId,
        Reason? reason,
        DateTimeOffset now
    )
    {
        throw new NotImplementedException();
    }

    public Result Cancel(
        EmployeeId actorId,
        DateOnly today,
        Reason? reason,
        DateTimeOffset now
    )
    {
        throw new NotImplementedException();
    }
}