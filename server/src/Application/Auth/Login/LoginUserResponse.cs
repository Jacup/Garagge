namespace Application.Auth.Login;

public sealed record LoginUserResponse
{
    public required string AccessToken { get; init; }
}
