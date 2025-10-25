using ApiIntegrationTests.Contracts.V1;
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
    private const string UserEmail = "j.doe@garagge.app";
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string UserPassword = "Password123";

    public AuthFlowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AuthFlow_RegisterThenLogin_Success()
    {
        var registerRequest = new AuthRegisterRequest("test@example.com", "Password123", "John", "Doe");
        var registerResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, registerRequest);
        registerResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();
        userId.ShouldNotBe(Guid.Empty);

        var loginRequest = new AuthLoginRequest("test@example.com", "Password123");
        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, loginRequest);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>();
        loginResult?.AccessToken.ShouldNotBeNull();
    }

    [Fact]
    public async Task AuthFlow_RegisterDuplicateEmailThenLogin_FirstUserCanLogin()
    {
        await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, new AuthRegisterRequest("duplicate@test.com", "Password123", "John", "Doe"));

        var duplicateResponse =
            await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, new AuthRegisterRequest("duplicate@test.com", "Password456", "Jane", "Smith"));
        duplicateResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest("duplicate@test.com", "Password123"));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthFlow_RegisterWithInvalidDataThenLogin_RegisterFailsLoginUnnecessary()
    {
        var registerResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, new AuthRegisterRequest("test@example.com", "123", "John", "Doe"));
        registerResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest("test@example.com", "123"));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthFlow_RegisterThenLoginWithWrongPassword_LoginFails()
    {
        await Client.PostAsJsonAsync(ApiV1Definition.Auth.Register, new AuthRegisterRequest("test@example.com", "Password123", "John", "Doe"));

        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest("test@example.com", "WrongPassword"));
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #region ChangePassword Flow Tests

    private const string NewPassword = "NewPassword123";

    [Fact]
    public async Task ChangePasswordFlow_ValidChangePasswordRequest_Success()
    {
        // Step 1: Register and login user
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: Change password
        var changePasswordRequest = new AuthChangePasswordRequest(UserPassword, NewPassword);
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definition.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 3: Verify the old password no longer works
        var oldPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest(UserEmail, UserPassword));
        oldPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Step 4: Verify the new password works
        var newPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest(UserEmail, NewPassword));
        newPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePasswordFlow_WrongCurrentPassword_ShouldFail()
    {
        // Step 1: Register and login user
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: Attempt to change password with wrong current password
        var changePasswordRequest = new AuthChangePasswordRequest("WrongCurrentPassword", NewPassword);
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definition.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Step 3: Verify the original password still works
        var originalPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest(UserEmail, UserPassword));
        originalPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePasswordFlow_SamePasswordAsOld_ShouldFail()
    {
        // Step 1: Register and login user
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: Attempt to change the password to the same password
        var changePasswordRequest = new AuthChangePasswordRequest(UserPassword, UserPassword);
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definition.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Step 3: Verify the original password still works
        var originalPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest(UserEmail, UserPassword));
        originalPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePasswordFlow_UnauthenticatedUser_ShouldFail()
    {
        // Step 1: 
        await RegisterUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: Attempt to change password without authentication
        var changePasswordRequest = new AuthChangePasswordRequest("CurrentPass123", "NewPassword456");
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definition.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("", "ValidNewPass123")] // Empty current password
    [InlineData("ValidCurrentPass123", "")] // Empty new password
    [InlineData("short", "ValidNewPass123")] // Current password too short
    [InlineData("ValidCurrentPass123", "short")] // New password too short 
    [InlineData("1234567", "ValidNewPass123")] // Exactly 7 characters (below a minimum)
    [InlineData("ValidCurrentPass123", "1234567")] // Exactly 7 characters (below a minimum)
    public async Task ChangePasswordFlow_InvalidPasswordFormat_ShouldFail(string currentPassword, string newPassword)
    {
        // Step 1: Register and login user with a valid password
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: Attempt to change password with an invalid format
        var changePasswordRequest = new AuthChangePasswordRequest(currentPassword, newPassword);
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definition.Auth.ChangePassword, changePasswordRequest);

        changeResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Step 3: Verify the original password still works
        var originalPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest(UserEmail, UserPassword));
        originalPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePasswordFlow_MultipleConsecutiveChanges_ShouldWork()
    {
        // Step 1: Register and login user
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Step 2: First password change
        const string secondPassword = "SecondPass456";
        var firstChange = new AuthChangePasswordRequest(UserPassword, secondPassword);
        var firstResponse = await Client.PutAsJsonAsync(ApiV1Definition.Auth.ChangePassword, firstChange);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 3: Second password change (using the new password as current)
        const string thirdPassword = "ThirdPass789";
        var secondChange = new AuthChangePasswordRequest(secondPassword, thirdPassword);
        var secondResponse = await Client.PutAsJsonAsync(ApiV1Definition.Auth.ChangePassword, secondChange);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 4: Verify only the latest password works
        var latestPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest(UserEmail, thirdPassword));
        latestPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 5: Verify old passwords don't work
        var originalPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest(UserEmail, UserPassword));
        originalPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var secondPasswordLogin = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, new AuthLoginRequest(UserEmail, secondPassword));
        secondPasswordLogin.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // not implemented yet - requires concurrency handling in the application
    // [Fact]
    // public async Task ChangePasswordFlow_ConcurrentPasswordChanges_OneShouldSucceed()
    // {
    //     // This test checks for race conditions in password change operations
    //
    //     // Step 1: Register and login user
    //     await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
    //
    //     // Step 2: Simulate concurrent password change requests
    //     var changeRequest1 = new ChangePasswordRequest(UserPassword, "NewPassword123");
    //     var changeRequest2 = new ChangePasswordRequest(UserPassword, "DifferentPassword456");
    //
    //     var task1 = Client.PutAsJsonAsync(ApiV1Definition.Auth.ChangePassword, changeRequest1);
    //     var task2 = Client.PutAsJsonAsync(ApiV1Definition.Auth.ChangePassword, changeRequest2);
    //
    //     var responses = await Task.WhenAll(task1, task2);
    //
    //     // Step 3: At least one should succeed (depending on implementation)
    //     var successfulResponses = responses.Where(r => r.StatusCode == HttpStatusCode.OK).ToList();
    //     var failedResponses = responses.Where(r => r.StatusCode != HttpStatusCode.OK).ToList();
    //
    //     // Business rule: Only one concurrent change should succeed
    //     successfulResponses.Count.ShouldBe(1);
    //     failedResponses.Count.ShouldBe(1);
    // }

    #endregion
}