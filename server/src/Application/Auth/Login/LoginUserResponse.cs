namespace Application.Auth.Login;

public sealed record LoginUserResponse(string AccessToken, string RefreshToken);