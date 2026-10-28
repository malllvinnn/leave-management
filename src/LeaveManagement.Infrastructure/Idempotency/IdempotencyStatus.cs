namespace LeaveManagement.Infrastructure.Idempotency;

public enum IdempotencyStatus
{
    InProgress,
    Completed,
    Failed
}