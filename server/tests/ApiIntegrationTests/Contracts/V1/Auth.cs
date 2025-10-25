namespace ApiIntegrationTests.Contracts.V1;

internal sealed record AuthLoginRequest(string Email, string Password);

internal sealed record AuthRegisterRequest(string Email, string Password, string FirstName, string LastName);

internal sealed record AuthChangePasswordRequest(string CurrentPassword, string NewPassword);