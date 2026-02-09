using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Fixtures;
using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using Application.Features.Users;

namespace ApiIntegrationTests.Flows;

[Collection("UserFlowTests")]
public class UserFlowTests : BaseIntegrationTest
{
    private const string UserEmail = "user@garagge.app";
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string UserPassword = "Password123";

    public UserFlowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region Get Profile Flow - Positive Tests

    [Fact]
    public async Task GetProfile_AuthenticatedUser_Success()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Act
        var response = await Client.GetAsync(ApiV1Definitions.Users.GetMe);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserDto>();

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Email.ShouldBe(UserEmail);
        result.FirstName.ShouldBe(FirstName);
        result.LastName.ShouldBe(LastName);
    }

    [Fact]
    public async Task GetProfile_MultipleTimes_ConsistentData()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Act
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => Client.GetAsync(ApiV1Definitions.Users.GetMe))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<UserDto>();
            result.ShouldNotBeNull();
            result.Email.ShouldBe(UserEmail);
            result.FirstName.ShouldBe(FirstName);
            result.LastName.ShouldBe(LastName);
        }
    }

    [Fact]
    public async Task GetProfile_TwoUsers_DataIsolation()
    {
        // Arrange - User A
        await RegisterUser("usera@garagge.app", "User", "A", UserPassword);
        await LoginUser("usera@garagge.app", UserPassword);

        var responseA = await Client.GetAsync(ApiV1Definitions.Users.GetMe);
        var dataA = await responseA.Content.ReadFromJsonAsync<UserDto>();

        // Arrange - User B (new client with separate cookies)
        using var userBClient = Factory.CreateClient();

        await RegisterUser("userb@garagge.app", "User", "B", UserPassword);

        var loginRequest = new LoginRequest("userb@garagge.app", UserPassword, false);
        await userBClient.PostAsJsonAsync(ApiV1Definitions.Auth.Login, loginRequest);

        var responseB = await userBClient.GetAsync(ApiV1Definitions.Users.GetMe);
        var dataB = await responseB.Content.ReadFromJsonAsync<UserDto>();

        // Assert
        responseA.StatusCode.ShouldBe(HttpStatusCode.OK);
        responseB.StatusCode.ShouldBe(HttpStatusCode.OK);

        dataA.ShouldNotBeNull();
        dataA.Email.ShouldBe("usera@garagge.app");

        dataB.ShouldNotBeNull();
        dataB.Email.ShouldBe("userb@garagge.app");
        dataA.Id.ShouldNotBe(dataB.Id);
    }

    #endregion

    #region Get Profile Flow - Negative Tests

    [Fact]
    public async Task GetProfile_Unauthorized_ShouldReturn401()
    {
        // Act
        var response = await Client.GetAsync(ApiV1Definitions.Users.GetMe);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_AfterLogout_ShouldReturn401()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        await Client.PostAsync(ApiV1Definitions.Auth.Logout, null);

        // Act
        var response = await Client.GetAsync(ApiV1Definitions.Users.GetMe);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_AfterAccountDeletion_ShouldReturn401()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var deleteResponse = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMe);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var response = await Client.GetAsync(ApiV1Definitions.Users.GetMe);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Update Profile Flow - Positive Tests

    [Fact]
    public async Task UpdateProfile_ValidData_Success()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        const string newFirstName = "Jane";
        const string newLastName = "Smith";
        const string newEmail = "jane.smith@garagge.app";

        // Act
        var updateRequest = new UserUpdateMeRequest(newEmail, newFirstName, newLastName);
        var updateResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Users.UpdateMe, updateRequest);

        // Assert
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<UserDto>();
        updateResult.ShouldNotBeNull();
        updateResult.Email.ShouldBe(newEmail);
        updateResult.FirstName.ShouldBe(newFirstName);
        updateResult.LastName.ShouldBe(newLastName);

        var user = DbContext.Users.First();
        user.Email.ShouldBe(newEmail);
        user.FirstName.ShouldBe(newFirstName);
        user.LastName.ShouldBe(newLastName);

        // Verify changes persisted
        var getMeResponse = await Client.GetAsync(ApiV1Definitions.Users.GetMe);

        getMeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var persistedResult = await getMeResponse.Content.ReadFromJsonAsync<UserDto>();
        persistedResult.ShouldNotBeNull();
        persistedResult.Email.ShouldBe(newEmail);
        persistedResult.FirstName.ShouldBe(newFirstName);
        persistedResult.LastName.ShouldBe(newLastName);
    }

    [Fact]
    public async Task UpdateProfile_SameEmail_Success()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        const string newFirstName = "UpdatedFirst";

        // Act - Update with the same email but different name
        var updateRequest = new UserUpdateMeRequest(UserEmail, newFirstName, LastName);
        var response = await Client.PutAsJsonAsync(ApiV1Definitions.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UserDto>();

        result.ShouldNotBeNull();
        result.Email.ShouldBe(UserEmail);
        result.FirstName.ShouldBe(newFirstName);
    }

    #endregion

    #region Update Profile Flow - Negative Tests

    [Fact]
    public async Task UpdateProfile_Unauthorized_ShouldReturn401()
    {
        // Arrange
        var updateRequest = new UserUpdateMeRequest("test@garagge.app", "Test", "User");

        // Act
        var response = await Client.PutAsJsonAsync(ApiV1Definitions.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_InvalidEmail_ShouldReturn400()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var updateRequest = new UserUpdateMeRequest("invalid-email", FirstName, LastName);

        // Act
        var response = await Client.PutAsJsonAsync(ApiV1Definitions.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Verify profile unchanged
        var getMeResponse = await Client.GetAsync(ApiV1Definitions.Users.GetMe);

        var result = await getMeResponse.Content.ReadFromJsonAsync<UserDto>();

        result.ShouldNotBeNull();
        result.Email.ShouldBe(UserEmail);
    }

    [Fact]
    public async Task UpdateProfile_EmptyFirstName_ShouldReturn400()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var updateRequest = new UserUpdateMeRequest(UserEmail, "", LastName);

        // Act
        var response = await Client.PutAsJsonAsync(ApiV1Definitions.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_EmptyLastName_ShouldReturn400()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var updateRequest = new UserUpdateMeRequest(UserEmail, FirstName, "");

        // Act
        var response = await Client.PutAsJsonAsync(ApiV1Definitions.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_DuplicateEmail_ShouldReturn409()
    {
        // Arrange
        await RegisterUser("user1@garagge.app", "User", "One", UserPassword);

        await RegisterAndAuthenticateUser("user2@garagge.app", "User", "Two", UserPassword);

        // Act - Try to update user2's email to user1's email
        var updateRequest = new UserUpdateMeRequest("user1@garagge.app", "User", "Two");
        var response = await Client.PutAsJsonAsync(ApiV1Definitions.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateProfile_NotAuthenticated_ShouldReturn401()
    {
        // Arrange
        await RegisterUser(UserEmail, FirstName, LastName, UserPassword);

        // Act
        var updateRequest = new UserUpdateMeRequest("new@garagge.app", "New", "User");
        var response = await Client.PutAsJsonAsync(ApiV1Definitions.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_AfterAccountDeletion_ShouldReturn401()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var deleteResponse = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMe);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var updateRequest = new UserUpdateMeRequest("new@garagge.app", "New", "User");
        var response = await Client.PutAsJsonAsync(ApiV1Definitions.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Delete Profile Flow - Positive Tests

    [Fact]
    public async Task DeleteProfile_AuthenticatedUser_Success()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        var user = await DbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == UserEmail);
        user.ShouldNotBeNull();

        var rtExists = await DbContext.RefreshTokens.AsNoTracking().AnyAsync(rt => rt.UserId == user.Id);
        rtExists.ShouldBeTrue();

        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMe);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var userExists = await DbContext.Users.AsNoTracking().AnyAsync(u => u.Id == user.Id);
        userExists.ShouldBeFalse();

        var rtExistsAfterDeleteUser = await DbContext.RefreshTokens.AsNoTracking().AnyAsync(rt => rt.UserId == user.Id);
        rtExistsAfterDeleteUser.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteProfile_ThenRegisterWithSameEmail_Success()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        var deleteResponse = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMe);

        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        await RegisterUser(UserEmail, FirstName, LastName, UserPassword);
    }

    #endregion

    #region Delete Profile Flow - Negative Tests

    [Fact]
    public async Task DeleteProfile_Unauthorized_ShouldReturn401()
    {
        // Act
        var response = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMe);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteProfile_Twice_SecondShouldReturn401()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var response1 = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMe);
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var response2 = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMe);

        // Assert
        response2.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteProfile_ThenLogin_ShouldFail()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var deleteResponse = await Client.DeleteAsync(ApiV1Definitions.Users.DeleteMe);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var loginRequest = new LoginRequest(UserEmail, UserPassword, false);
        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definitions.Auth.Login, loginRequest);

        // Assert
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
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
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Users.ChangePassword, changePasswordRequest);

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
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Users.ChangePassword, changePasswordRequest);

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
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Users.ChangePassword, changePasswordRequest);

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
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Users.ChangePassword, changePasswordRequest);

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
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Users.ChangePassword, changePasswordRequest);

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
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Users.ChangePassword, changePasswordRequest);

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
        var firstResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Users.ChangePassword, firstChange);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 3: Second password change (using the new password as current)
        const string thirdPassword = "ThirdPass789";
        var secondChange = new ChangePasswordRequest(secondPassword, thirdPassword);
        var secondResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Users.ChangePassword, secondChange);
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
        var changeResponse = await Client.PutAsJsonAsync(ApiV1Definitions.Users.ChangePassword, changePasswordRequest);
        changeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 3: Current session should still be able to refresh
        var refreshResponse = await Client.PostAsync(ApiV1Definitions.Auth.Refresh, null);
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    #endregion


    #region Edge Cases

    [Theory]
    [InlineData("", "Valid", "User")] // Empty email
    [InlineData("invalid-email", "Valid", "User")] // Invalid email format
    [InlineData("valid@email.com", "", "User")] // Empty first name
    [InlineData("valid@email.com", "Valid", "")] // Empty last name
    public async Task UpdateProfile_InvalidData_ShouldReturn400(string email, string firstName, string lastName)
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var updateRequest = new UserUpdateMeRequest(email, firstName, lastName);

        // Act
        var response = await Client.PutAsJsonAsync(ApiV1Definitions.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Verify original data unchanged
        var getMeResponse = await Client.GetAsync(ApiV1Definitions.Users.GetMe);

        var result = await getMeResponse.Content.ReadFromJsonAsync<UserDto>();
        result.ShouldNotBeNull();
        result.Email.ShouldBe(UserEmail);
        result.FirstName.ShouldBe(FirstName);
        result.LastName.ShouldBe(LastName);
    }

    #endregion
}