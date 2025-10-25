namespace ApiIntegrationTests.Contracts.V1;

internal sealed record UserUpdateMeRequest(string Email, string FirstName, string LastName);