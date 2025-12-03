using Domain.Entities.Users;

namespace Domain.Entities.Auth;

public class RefreshToken : Entity
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceName { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}