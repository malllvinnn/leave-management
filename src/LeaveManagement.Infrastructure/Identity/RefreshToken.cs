namespace LeaveManagement.Infrastructure.Identity;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string Jti { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}