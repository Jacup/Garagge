namespace ApiIntegrationTests.Contracts.V1;

internal sealed record UserUpdateMeRequest(string Email, string FirstName, string LastName);

internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, bool LogoutAllDevices = false);
