using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

public abstract class LeaveRequestTestBase
{
    protected EmployeeId _employeeId;
    protected LeaveDays _fiveWorkingDays;
    protected LeaveAllocation _threeAnnualPlusTwoCarryOverAllocation;
    protected LeaveType _annualType;
    protected Reason _validReason;
    protected DateOnly _earlyMarchToday;
    protected DateTimeOffset _submittedAtPlusSeven;
    protected DateTimeOffset _submittedAtJustAfterNewYearUtcPlusSeven;
    protected EmployeeId _approverId;
    protected Reason _validApprovalReason;
    protected DateTimeOffset _firstApprovalAt;
    protected DateTimeOffset _finalApprovalAt;
    protected Reason _validApprovalFinalReason;
    protected EmployeeId _actorReject;
    protected Reason _validRejectionReason;
    protected DateTimeOffset _rejectionAt;
    protected Reason _validCancellationReason;
    protected DateTimeOffset _cancellationAt;
    protected Reason _validOverrideReason;
    protected EmployeeId _overrideActorId;
    protected DateTimeOffset _overrideAt;
    protected EmployeeId _validBypassedId;
    protected DateTimeOffset _thirdMarchAt;
    protected DateTimeOffset _fourthMarchAt;
    protected DateOnly _thirdMarchToday;

    [SetUp]
    public void Setup()
    {
        _employeeId = EmployeeId.New();
        _fiveWorkingDays = LeaveDays.Create(5).Value;

        _threeAnnualPlusTwoCarryOverAllocation = LeaveAllocation.Create(
            annual: LeaveDays.Create(3).Value,
            carryOver: LeaveDays.Create(2).Value
        ).Value;

        _annualType = LeaveType.AnnualLeave;
        _validReason = Reason.Create("valid reason").Value;
        _earlyMarchToday = new DateOnly(2026, 3, 1);

        _submittedAtPlusSeven = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 1,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _submittedAtJustAfterNewYearUtcPlusSeven = new DateTimeOffset(
            year: 2026,
            month: 1,
            day: 1,
            hour: 0,
            minute: 30,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _approverId = EmployeeId.New();
        _validApprovalReason = Reason.Create("Approved as scheduled").Value;

        _firstApprovalAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 2,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _finalApprovalAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 3,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _validApprovalFinalReason = Reason.Create("Final approval granted").Value;
        _actorReject = EmployeeId.New();
        _validRejectionReason = Reason.Create("Request cannot be approved").Value;

        _rejectionAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 4,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _validCancellationReason = Reason.Create("Plans have changed").Value;

        _cancellationAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 15,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _validOverrideReason = Reason.Create("Valid override reason").Value;
        _overrideActorId = EmployeeId.New();

        _overrideAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 4,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _validBypassedId = EmployeeId.New();

        _thirdMarchAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 3,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _fourthMarchAt = new DateTimeOffset(
            year: 2026,
            month: 3,
            day: 4,
            hour: 10,
            minute: 0,
            second: 0,
            offset: TimeSpan.FromHours(7)
        );

        _thirdMarchToday = new DateOnly(2026, 3, 3);
    }

    protected sealed record DecisionSnapshot(
        Guid Id,
        LeaveRequestId LeaveRequestId,
        EmployeeId ActorId,
        DecisionType Type,
        ApprovalStage Stage,
        Reason? Reason,
        EmployeeId? BypassedApproverId,
        DecisionType? OverrideOutcome,
        DateTimeOffset DecidedAt,
        TimeSpan DecidedAtOffset
    );

    protected static DecisionSnapshot[] CaptureDecisionHistory(LeaveRequest request)
    {
        var result = request.Decisions.Select((decision) => new DecisionSnapshot(
            Id: decision.Id,
            LeaveRequestId: decision.LeaveRequestId,
            ActorId: decision.ActorId,
            Type: decision.Type,
            Stage: decision.Stage,
            Reason: decision.Reason,
            BypassedApproverId: decision.BypassedApproverId,
            OverrideOutcome: decision.OverrideOutcome,
            DecidedAt: decision.DecidedAt,
            DecidedAtOffset: decision.DecidedAt.Offset
        )).ToArray();

        return result;
    }

    protected Result<LeaveRequest> CreatePendingRequestForStage(ApprovalStage stage)
    {
        var endPeriod = stage is ApprovalStage.First
            ? new DateOnly(2026, 3, 20)
            : new DateOnly(2026, 3, 23);

        var period = DateRange.Create(
            start: new DateOnly(2026, 3, 16),
            end: endPeriod
        ).Value;

        var workingDays = stage is ApprovalStage.First
            ? LeaveDays.Create(5).Value
            : LeaveDays.Create(6).Value;

        var allocation = LeaveAllocation.Create(
            annual: workingDays,
            carryOver: LeaveDays.Zero
        ).Value;

        var createResult = LeaveRequest.Create(
            employeeId: _employeeId,
            period: period,
            workingDays: workingDays,
            allocation: allocation,
            type: _annualType,
            reason: _validReason,
            today: _earlyMarchToday,
            submittedAt: _submittedAtPlusSeven
        );

        return createResult;
    }
}
