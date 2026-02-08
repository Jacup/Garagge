namespace Application.Features.Auth;

public sealed record LoginUserResponse(string AccessToken, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);