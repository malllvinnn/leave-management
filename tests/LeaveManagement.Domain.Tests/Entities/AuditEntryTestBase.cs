using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Tests.Entities;

public abstract class AuditEntryTestBase
{
    protected static Result<AuditEntry> CreateEntry(
        string? entityType = "LeaveRequest",
        string? entityId = "3f2504e0-4f89-11d3-9a0c-0305e82c3301",
        string? action = "Approved",
        string? payload = "{\"status\":\"Approved\"}",
        string? actorSnapshot = "Sari — Backend Developer — Employee",
        EmployeeId? actorId = null,
        DateTimeOffset? occurredAt = null
    )
    {
        var resolvedOccurredAt = occurredAt
            ?? new DateTimeOffset(
                year: 2026,
                month: 3,
                day: 4,
                hour: 10,
                minute: 0,
                second: 0,
                offset: TimeSpan.FromHours(7)
            );

        var createResult = AuditEntry.Create(
            entityType: entityType,
            entityId: entityId,
            action: action,
            payload: payload,
            actorSnapshot: actorSnapshot,
            actorId: actorId,
            occurredAt: resolvedOccurredAt
        );

        return createResult;
    }
}