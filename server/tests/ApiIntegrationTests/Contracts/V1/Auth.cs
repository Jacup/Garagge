namespace ApiIntegrationTests.Contracts.V1;

internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, bool LogoutAllDevices = false);

internal sealed record LoginRequest(string Email, string Password, bool RememberMe);

internal sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName);