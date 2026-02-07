using ApiIntegrationTests.Contracts;
using ApiIntegrationTests.Contracts.V1;
using ApiIntegrationTests.Fixtures;
using Application.Users;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

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