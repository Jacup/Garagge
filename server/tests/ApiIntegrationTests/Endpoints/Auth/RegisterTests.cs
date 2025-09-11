using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Extensions;
using ApiIntegrationTests.Fixtures;
using Application.Auth;
using Application.Auth.Register;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Endpoints.Auth;

[Collection("RegisterTests")]
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
        
        var user = await DbContext.Users.FindAsync(userId);
        
        user.ShouldNotBeNull();
        user.Id.ShouldBe(userId);
        user.FirstName.ShouldBe(request.FirstName);
        user.LastName.ShouldBe(request.LastName);
        user.Email.ShouldBe(request.Email);
        user.PasswordHash.ShouldNotBeNullOrEmpty();
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
        
        CustomProblemDetails problemDetails = await response.GetProblemDetailsAsync();
        problemDetails.Title.ShouldBe(AuthErrors.EmailNotUnique.Code);
    }
    
    [Fact]
    public async Task Register_MissingEmail_ReturnsBadRequest()
    {
        // Act
        var request = new RegisterUserCommand("", "John", "Doe", "Password123");

        // Act
        var response = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        CustomProblemDetails problemDetails = await response.GetProblemDetailsAsync();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(AuthErrors.MissingEmail);
        problemDetails.Errors.ShouldContain(AuthErrors.InvalidEmail);
        
        DbContext.Users.Count().ShouldBe(0);
    }
}