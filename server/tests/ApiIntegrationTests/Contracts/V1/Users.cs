namespace ApiIntegrationTests.Contracts.V1;

internal sealed record UpdateMeRequest(string Email, string FirstName, string LastName);