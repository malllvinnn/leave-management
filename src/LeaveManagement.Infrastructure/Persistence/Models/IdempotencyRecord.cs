using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Infrastructure.Persistence.Models;

public class IdempotencyRecord
{
    public Guid Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public IdempotencyStatus Status { get; set; }
    public string? ResultEntityType { get; set; }
    public string? ResultEntityId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}