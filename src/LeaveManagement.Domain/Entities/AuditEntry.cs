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
        if (string.IsNullOrWhiteSpace(entityType))
        {
            var failureResult = Result<AuditEntry>.Fail("Entity type cannot be empty");

            return failureResult;
        }

        if (string.IsNullOrWhiteSpace(entityId))
        {
            var failureResult = Result<AuditEntry>.Fail("Entity id cannot be empty");

            return failureResult;
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            var failureResult = Result<AuditEntry>.Fail("Action cannot be empty");

            return failureResult;
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            var failureResult = Result<AuditEntry>.Fail("Payload cannot be empty");

            return failureResult;
        }

        var normalizedActorSnapshot = actorSnapshot?.Trim();
        var normalizedEntityType = entityType.Trim();
        var normalizedEntityId = entityId.Trim();
        var normalizedAction = action.Trim();

        if (normalizedEntityType.Length > 100)
        {
            var failureResult = Result<AuditEntry>.Fail("Entity type cannot exceed 100 characters");

            return failureResult;
        }

        if (normalizedEntityId.Length > 100)
        {
            var failureResult = Result<AuditEntry>.Fail("Entity id cannot exceed 100 characters");

            return failureResult;
        }

        if (normalizedAction.Length > 100)
        {
            var failureResult = Result<AuditEntry>.Fail("Action cannot exceed 100 characters");

            return failureResult;
        }

        if (normalizedActorSnapshot is not null && normalizedActorSnapshot.Length > 300)
        {
            var failureResult = Result<AuditEntry>.Fail("Actor snapshot cannot exceed 300 characters");

            return failureResult;
        }

        var utcOccurredAt = occurredAt.ToUniversalTime();

        var auditEntry = new AuditEntry(
            id: Guid.NewGuid(),
            entityType: normalizedEntityType,
            entityId: normalizedEntityId,
            action: normalizedAction,
            actorId: actorId,
            actorSnapshot: normalizedActorSnapshot,
            payload: payload,
            occurredAt: utcOccurredAt
        );

        var successResult = Result<AuditEntry>.Ok(auditEntry);

        return successResult;
    }
}