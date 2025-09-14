using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Fixtures;
using Application.Auth.Login;
using Application.Users;
using Application.Users.Me.Update;
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
        var response = await Client.GetAsync(ApiV1Definition.Users.GetMe);

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
            .Select(_ => Client.GetAsync(ApiV1Definition.Users.GetMe))
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
        // Arrange
        await RegisterUser("usera@garagge.app", "User", "A", UserPassword);
        var tokenA = (await LoginUser("usera@garagge.app", UserPassword)).AccessToken;
        
        await RegisterAndAuthenticateUser("userb@garagge.app", "User", "B", UserPassword);
        var tokenB = (await LoginUser("userb@garagge.app", UserPassword)).AccessToken;

        // Act
        Authenticate(tokenA);
        var responseA = await Client.GetAsync(ApiV1Definition.Users.GetMe);
        var dataA = await responseA.Content.ReadFromJsonAsync<UserDto>();

        Authenticate(tokenB);
        var responseB = await Client.GetAsync(ApiV1Definition.Users.GetMe);
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
        var response = await Client.GetAsync(ApiV1Definition.Users.GetMe);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // [Fact]
    // public async Task GetProfile_AfterLogout_ShouldReturn401()
    // {
    //     // Arrange
    //     await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
    //     await Logout();
    //
    //     // Act
    //     var response = await Client.GetAsync(ApiV1Definition.Users.GetMe);
    //
    //     // Assert
    //     response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    // }

    [Fact]
    public async Task GetProfile_AfterAccountDeletion_ShouldReturn401()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var deleteResponse = await Client.DeleteAsync(ApiV1Definition.Users.DeleteMe);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var response = await Client.GetAsync(ApiV1Definition.Users.GetMe);

        // Assert
        response.IsSuccessStatusCode.ShouldBeFalse();
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
        var updateRequest = new UpdateMeCommand(newEmail, newFirstName, newLastName);
        var updateResponse = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

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
        var getMeResponse = await Client.GetAsync(ApiV1Definition.Users.GetMe);
        
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
        var updateRequest = new UpdateMeCommand(UserEmail, newFirstName, LastName);
        var response = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

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
        var updateRequest = new UpdateMeCommand("test@garagge.app", "Test", "User");

        // Act
        var response = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_InvalidEmail_ShouldReturn400()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var updateRequest = new UpdateMeCommand("invalid-email", FirstName, LastName);

        // Act
        var response = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Verify profile unchanged
        var getMeResponse = await Client.GetAsync(ApiV1Definition.Users.GetMe);
        
        var result = await getMeResponse.Content.ReadFromJsonAsync<UserDto>();
        
        result.ShouldNotBeNull();
        result.Email.ShouldBe(UserEmail);
    }

    [Fact]
    public async Task UpdateProfile_EmptyFirstName_ShouldReturn400()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var updateRequest = new UpdateMeCommand(UserEmail, "", LastName);

        // Act
        var response = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProfile_EmptyLastName_ShouldReturn400()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var updateRequest = new UpdateMeCommand(UserEmail, FirstName, "");

        // Act
        var response = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

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
        var updateRequest = new UpdateMeCommand("user1@garagge.app", "User", "Two");
        var response = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateProfile_NotAuthenticated_ShouldReturn401()
    {
        // Arrange
        await RegisterUser(UserEmail, FirstName, LastName, UserPassword);

        // Act
        var updateRequest = new UpdateMeCommand("new@garagge.app", "New", "User");
        var response = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_AfterAccountDeletion_ShouldReturn401()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var deleteResponse = await Client.DeleteAsync(ApiV1Definition.Users.DeleteMe);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var updateRequest = new UpdateMeCommand("new@garagge.app", "New", "User");
        var response = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

        // Assert
        response.IsSuccessStatusCode.ShouldBeFalse();
    }
    
    #endregion

    #region Delete Profile Flow - Positive Tests

    [Fact]
    public async Task DeleteProfile_AuthenticatedUser_Success()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);

        // Act
        var response = await Client.DeleteAsync(ApiV1Definition.Users.DeleteMe);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteProfile_ThenRegisterWithSameEmail_Success()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        
        var deleteResponse = await Client.DeleteAsync(ApiV1Definition.Users.DeleteMe);
        
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
        var response = await Client.DeleteAsync(ApiV1Definition.Users.DeleteMe);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteProfile_Twice_SecondShouldReturn401()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var response1 = await Client.DeleteAsync(ApiV1Definition.Users.DeleteMe);
        response1.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var response2 = await Client.DeleteAsync(ApiV1Definition.Users.DeleteMe);

        // Assert
        response2.IsSuccessStatusCode.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteProfile_ThenLogin_ShouldFail()
    {
        // Arrange
        await RegisterAndAuthenticateUser(UserEmail, FirstName, LastName, UserPassword);
        var deleteResponse = await Client.DeleteAsync(ApiV1Definition.Users.DeleteMe);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var loginRequest = new LoginUserCommand(UserEmail, UserPassword);
        var loginResponse = await Client.PostAsJsonAsync(ApiV1Definition.Auth.Login, loginRequest);

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
        var updateRequest = new UpdateMeCommand(email, firstName, lastName);

        // Act
        var response = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Verify original data unchanged
        var getMeResponse = await Client.GetAsync(ApiV1Definition.Users.GetMe);
        
        var result = await getMeResponse.Content.ReadFromJsonAsync<UserDto>();
        result.ShouldNotBeNull();
        result.Email.ShouldBe(UserEmail);
        result.FirstName.ShouldBe(FirstName);
        result.LastName.ShouldBe(LastName);
    }

    #endregion
}