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
        tokens.Count.ShouldBe(2); // oba tokeny pozostają w DB
        tokens.ShouldAllBe(t => t.IsRevoked); // oba są revoked
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

    #region ChangePassword Flow Tests

    private const string NewPassword = "NewPassword123";

    [Fact]
    public async Task ChangePasswordFlow_ValidChangePasswordRequest_Success()
    {
        // Step 1: Register and login user
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Create an additional refresh token to simulate another device/session
        var user = await DbContext.Users.FirstAsync(u => u.Email == UserEmail);
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "SomeRefreshToken",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            SessionDurationDays = 10,
        };
        DbContext.RefreshTokens.Add(refreshToken);
        await DbContext.SaveChangesAsync();

        DbContext.RefreshTokens.Count(rt => rt.UserId == user.Id).ShouldBe(2);

        // Step 2: Change password without logout
        var changePasswordRequest = new ChangePasswordRequest(UserPassword, NewPassword, false);
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Both tokens should still exist (no logout)
        DbContext.RefreshTokens.Count(rt => rt.UserId == user.Id).ShouldBe(2);

        // Step 3: Verify the old password no longer works
        var oldPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest(UserEmail, UserPassword, false));
        oldPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Step 4: Verify the new password works
        var newPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest(UserEmail, NewPassword, false));
        newPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePasswordFlow_ValidChangePasswordRequestWithLogout_Success()
    {
        // Step 1: Register and login user
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Create an additional refresh token to simulate another device/session
        var user = await DbContext.Users.FirstAsync(u => u.Email == UserEmail);
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "SomeRefreshToken",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            SessionDurationDays = 10,
        };
        DbContext.RefreshTokens.Add(refreshToken);
        await DbContext.SaveChangesAsync();

        DbContext.RefreshTokens.Count(rt => rt.UserId == user.Id).ShouldBe(2);

        // Step 2: Change password with logout (revokes all other sessions)
        var changePasswordRequest = new ChangePasswordRequest(UserPassword, NewPassword, true);
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // All tokens except current session should be revoked
        var activeTokens = await DbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync();
        activeTokens.Count.ShouldBe(1);

        // Step 3: Verify the old password no longer works
        var oldPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest(UserEmail, UserPassword, false));
        oldPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Step 4: Verify the new password works
        var newPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest(UserEmail, NewPassword, false));
        newPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePasswordFlow_WrongCurrentPassword_ShouldFail()
    {
        // Step 1: Register and login user
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: Attempt to change password with wrong current password
        var changePasswordRequest = new ChangePasswordRequest("WrongCurrentPassword", NewPassword);
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Step 3: Verify the original password still works
        var originalPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest(UserEmail, UserPassword, false));
        originalPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePasswordFlow_SamePasswordAsOld_ShouldFail()
    {
        // Step 1: Register and login user
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: Attempt to change the password to the same password
        var changePasswordRequest = new ChangePasswordRequest(UserPassword, UserPassword);
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Step 3: Verify the original password still works
        var originalPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest(UserEmail, UserPassword, false));
        originalPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePasswordFlow_UnauthenticatedUser_ShouldFail()
    {
        // Step 1: Register user but DON'T authenticate
        await RegisterUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: Attempt to change password without authentication
        var changePasswordRequest = new ChangePasswordRequest("CurrentPass123", "NewPassword456");
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("", "ValidNewPass123")] // Empty current password
    [InlineData("ValidCurrentPass123", "")] // Empty new password
    [InlineData("ValidCurrentPass123", "short")] // New password too short 
    [InlineData("ValidCurrentPass123", "1234567")] // Exactly 7 characters (below minimum)
    public async Task ChangePasswordFlow_InvalidPasswordFormat_ShouldFail(string currentPassword, string newPassword)
    {
        // Step 1: Register and login user with a valid password
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: Attempt to change password with an invalid format
        var changePasswordRequest = new ChangePasswordRequest(currentPassword, newPassword);
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Step 3: Verify the original password still works
        var originalPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest(UserEmail, UserPassword, false));
        originalPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePasswordFlow_MultipleConsecutiveChanges_ShouldWork()
    {
        // Step 1: Register and login user
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: First password change
        const string secondPassword = "SecondPass456";
        var firstChange = new ChangePasswordRequest(UserPassword, secondPassword);
        var firstResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Auth.ChangePassword, firstChange);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 3: Second password change (using the new password as current)
        const string thirdPassword = "ThirdPass789";
        var secondChange = new ChangePasswordRequest(secondPassword, thirdPassword);
        var secondResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Auth.ChangePassword, secondChange);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 4: Verify only the latest password works
        var latestPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest(UserEmail, thirdPassword, false));
        latestPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Step 5: Verify old passwords don't work
        var originalPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest(UserEmail, UserPassword, false));
        originalPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var secondPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, new LoginRequest(UserEmail, secondPassword, false));
        secondPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePasswordFlow_AfterChangeCanStillRefresh_Success()
    {
        // Step 1: Register and login
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: Change password (without logout)
        var changePasswordRequest = new ChangePasswordRequest(UserPassword, NewPassword, false);
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Auth.ChangePassword, changePasswordRequest);
        changeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 3: Current session should still be able to refresh
        var refreshResponse = await Client.PostAsync(ApiV1Definitions.Auth.Refresh, null);
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
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

    [Fact]
    public async Task TokenSecurityFlow_ExpiredAccessToken_RefreshesAutomatically()
    {
        // This would require mocking time or waiting for expiration
        // For now, document the expected behavior
        // TODO: Implement when time mocking is available
    }

    #endregion
}