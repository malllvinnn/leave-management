namespace LeaveManagement.Infrastructure.Persistence.Models;

public enum IdempotencyStatus
{
    InProgress,
    Completed,
    Failed
}