namespace ApiIntegrationTests.Contracts.V1;

internal sealed record LoginRequest(string Email, string Password);

internal sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName);

internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);