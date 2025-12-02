using Domain.Entities.Users;

namespace Domain.Entities.Auth;

public class RefreshToken : Entity
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}