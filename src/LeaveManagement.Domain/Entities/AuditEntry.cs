using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Entities;

public class AuditEntry
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; }
    public string EntityId { get; private set; }
    public string Action { get; private set; }
    public EmployeeId? ActorId { get; private set; }
    public string? ActorSnapshot { get; private set; }
    public string Payload { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AuditEntry(
        Guid id,
        string entityType,
        string entityId,
        string action,
        EmployeeId? actorId,
        string? actorSnapshot,
        string payload,
        DateTimeOffset occurredAt
    )
    {
        Id = id;
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        ActorId = actorId;
        ActorSnapshot = actorSnapshot;
        Payload = payload;
        OccurredAt = occurredAt;
    }

    public static Result<AuditEntry> Create(
        string? entityType,
        string? entityId,
        string? action,
        EmployeeId? actorId,
        string? actorSnapshot,
        string? payload,
        DateTimeOffset occurredAt
    )
    {
        throw new NotImplementedException();
    }
}