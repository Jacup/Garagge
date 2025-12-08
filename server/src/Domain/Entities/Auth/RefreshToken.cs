using Domain.Entities.Users;

namespace Domain.Entities.Auth;

public class RefreshToken : Entity
{
    public required string Token { get; init; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public required int SessionDurationDays { get; init; }
    
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }

    public string? UserAgent { get; set; }
    public string? DeviceOs { get; set; }
    public string? DeviceBrowser { get; set; }
    public string? DeviceType { get; set; }

    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    
    public Guid UserId { get; init; }
    public User? User { get; set; }
}