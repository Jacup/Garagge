using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Fixtures;
using Application.Auth.Register;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Endpoints.Auth;

public class RegisterTests : BaseIntegrationTest
{
    public RegisterTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        // Act
        var request = new RegisterUserCommand("test@garagge.app", "John", "Doe", "Password123");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var userId = await response.Content.ReadFromJsonAsync<Guid>();
        userId.ShouldNotBe(Guid.Empty);
    }
    
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Act
        var request = new RegisterUserCommand("test-conflict@garagge.app", "John", "Doe", "Password123");

        await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);
        
        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}