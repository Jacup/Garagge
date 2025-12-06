using Domain.Entities.Auth;

namespace Application.Auth;

public sealed record LoginUserResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt);