using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Fixtures;
using Application.Auth.Login;
using Application.Auth.Register;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Flows;

[Collection("AuthFlowTests")]
public class AuthFlowTests : BaseIntegrationTest
{
    public AuthFlowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AuthFlow_RegisterThenLogin_Success()
    {
        var registerRequest = new RegisterUserCommand("test@example.com", "John", "Doe", "Password123");
        var registerResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, registerRequest);
        registerResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();
        userId.ShouldNotBe(Guid.Empty);

        var loginRequest = new LoginUserCommand("test@example.com", "Password123");
        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, loginRequest);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>();
        loginResult?.AccessToken.ShouldNotBeNull();
    }

    [Fact]
    public async Task AuthFlow_RegisterDuplicateEmailThenLogin_FirstUserCanLogin()
    {
        await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, new RegisterUserCommand("duplicate@test.com", "John", "Doe", "Password123"));

        var duplicateResponse =
            await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, new RegisterUserCommand("duplicate@test.com", "Jane", "Smith", "Password456"));
        duplicateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new LoginUserCommand("duplicate@test.com", "Password123"));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthFlow_RegisterWithInvalidDataThenLogin_RegisterFailsLoginUnnecessary()
    {
        var registerResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, new RegisterUserCommand("test@example.com", "John", "Doe", "123"));
        registerResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new LoginUserCommand("test@example.com", "123"));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthFlow_RegisterThenLoginWithWrongPassword_LoginFails()
    {
        await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, new RegisterUserCommand("test@example.com", "John", "Doe", "Password123"));

        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new LoginUserCommand("test@example.com", "WrongPassword"));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}