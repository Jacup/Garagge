namespace ApiIntegrationTests.Contracts.V1;

internal sealed record LoginRequest(string Email, string Password, bool RememberMe);
public sealed record LoginResponse(string AccessToken);

internal sealed record RefreshRequest();

internal sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName);

internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

