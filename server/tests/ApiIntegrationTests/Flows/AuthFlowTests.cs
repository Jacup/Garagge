using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Extensions;
using ApiIntegrationTests.Fixtures;
using Domain.Entities.Auth;
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace ApiIntegrationTests.Flows;

[Collection("AuthFlowTests")]
public class AuthFlowTests : BaseIntegrationTest
{
    private const string UserEmail = "j.doe@garagge.app";
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string UserPassword = "Password123";

    public AuthFlowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region Registration and Login Flow Tests

    [Fact]
    public async Task AuthFlow_RegisterThenLogin_Success()
    {
        var registerRequest = new RegisterRequest("test@example.com", "Password123", "John", "Doe");
        var registerResponse = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Register, registerRequest);
        registerResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();
        userId.ShouldNotBe(Guid.Empty);

        var loginRequest = new LoginRequest("test@example.com", "Password123", false);
        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, loginRequest);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify cookies are set
        loginResponse.Headers.ShouldContainCookie("accessToken");
        loginResponse.Headers.ShouldContainCookie("refreshToken");
    }

    [Fact]
    public async Task AuthFlow_RegisterDuplicateEmailThenLogin_FirstUserCanLogin()
    {
        await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Register, new RegisterRequest("duplicate@test.com", "Password123", "John", "Doe"));

        var duplicateResponse =
            await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Register, new RegisterRequest("duplicate@test.com", "Password456", "Jane", "Smith"));

        duplicateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest("duplicate@test.com", "Password123", false));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        loginResponse.Headers.ShouldContainCookie("accessToken");
    }

    [Fact]
    public async Task AuthFlow_RegisterWithInvalidDataThenLogin_RegisterFailsLoginUnnecessary()
    {
        var registerResponse = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Register, new RegisterRequest("test@example.com", "123", "John", "Doe"));
        registerResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest("test@example.com", "123", false));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthFlow_RegisterThenLoginWithWrongPassword_LoginFails()
    {
        await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Register, new RegisterRequest("test@example.com", "Password123", "John", "Doe"));

        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest("test@example.com", "PasswordInvalid", false));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthFlow_LoginLogoutLogin_Success()
    {
        // Register user
        await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Register, new RegisterRequest("test@example.com", "Password123", "John", "Doe"));

        // First login
        var loginResponse1 = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest("test@example.com", "Password123", false));
        loginResponse1.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Logout
        var logoutResponse = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);
        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Second login should work
        var loginResponse2 = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest("test@example.com", "Password123", false));
        loginResponse2.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        loginResponse2.Headers.ShouldContainCookie("accessToken");
    }

    [Fact]
    public async Task AuthFlow_LoginRefreshLogout_Success()
    {
        // Register and login
        var user = await CreateUserAsync();
        await LoginUser("test@garagge.app", "Password123");

        // Refresh tokens
        var refreshResponse = await Client.PostAsync(ApiV1Definitions.Auth.Refresh, null);
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Logout
        var logoutResponse = await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);
        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify all tokens for user are revoked
        var tokens = await DbContext.RefreshTokens.Where(rt => rt.UserId == user.Id).ToListAsync();
        tokens.Count.ShouldBe(2);
        tokens.ShouldAllBe(t => t.IsRevoked);
    }

    [Fact]
    public async Task AuthFlow_MultipleDeviceLogins_EachHasOwnTokens()
    {
        // Register user
        var user = await CreateUserAsync();

        // Login from device 1 (current client with cookie container)
        await LoginUser("test@garagge.app", "Password123");

        // Login from device 2 (new client)
        using var device2Client = Factory.CreateClient();
        var loginRequest = new LoginRequest("test@garagge.app", "Password123", false);
        var device2Response = await device2Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, loginRequest);
        device2Response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify both devices have separate refresh tokens
        var tokens = await DbContext.RefreshTokens.AsNoTracking().Where(rt => rt.UserId == user.Id && !rt.IsRevoked).ToListAsync();
        tokens.Count.ShouldBe(2);
    }

    #endregion
    
    #region Token Security Flow Tests

    [Fact]
    public async Task TokenSecurityFlow_LogoutInvalidatesCookies_CannotUseAfter()
    {
        // Login
        await CreateAndAuthenticateUser();

        // Logout
        await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);

        // Try to access protected endpoint - should fail
        var response = await Client.GetAsync("/api/users/me");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion
}