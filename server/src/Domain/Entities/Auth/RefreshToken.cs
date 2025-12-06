using Domain.Entities.Users;

namespace Domain.Entities.Auth;

public class RefreshToken : Entity
{
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; set; }
    public required int SessionDurationDays { get; init; }
    
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceName { get; set; }

    public Guid UserId { get; init; }
    public User? User { get; set; }
}