using ApiIntegrationTests.Definitions;
using ApiIntegrationTests.Fixtures;
using Application.Users;
using Application.Users.Me.Update;
using System.Net;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Flows;

public class UserFlowTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UserProfileFlow_RegisterLoginGetProfile_Success()
    {
        const string userEmail = "test@garagge.app";
        const string firstName = "John";
        const string lastName = "Doe";
        const string userPassword = "Password123";

        await RegisterAndAuthenticateUser(userEmail, firstName, lastName, userPassword);

        var getMeResponse = await Client.GetAsync(ApiV1Definition.Users.GetMe);

        getMeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await getMeResponse.Content.ReadFromJsonAsync<UserDto>();

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Email.ShouldBe(userEmail);
        result.FirstName.ShouldBe(firstName);
        result.LastName.ShouldBe(lastName);
    }

    [Fact]
    public async Task UserProfileFlow_LoginGetProfileMultipleTimes_ConsistentData()
    {
        const string userEmail = "test@garagge.app";
        const string firstName = "John";
        const string lastName = "Doe";
        const string userPassword = "Password123";

        await RegisterAndAuthenticateUser(userEmail, firstName, lastName, userPassword);

        var tasks = Enumerable.Range(0, 5)
            .Select(_ => Client.GetAsync(ApiV1Definition.Users.GetMe))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<UserDto>();

            result.ShouldNotBeNull();
            result.Id.ShouldNotBe(Guid.Empty);
            result.Email.ShouldBe(userEmail);
            result.FirstName.ShouldBe(firstName);
            result.LastName.ShouldBe(lastName);
        }
    }

    // [Fact]
    // public async Task UserProfileFlow_LoginGetProfileLogoutGetProfile_LogoutRevokesAccess()
    // {
    //     // Step 1: Register and login
    //     // Step 2: GET /api/users/me - should work
    //     // Step 3: Logout
    //     // Step 4: GET /api/users/me - should fail (401)
    // }

    //
    // [Fact]
    // public async Task UserProfileFlow_MultipleDevicesLogin_EachCanAccessProfile()
    // {
    //     // Step 1: Register user
    //     // Step 2: Login from "device 1" (get token1)
    //     // Step 3: Login from "device 2" (get token2)
    //     // Step 4: Both tokens can access /api/users/me
    //     // Assert: Both return same user data
    // }
    //
    // [Fact]
    // public async Task UserProfileFlow_LoginTokenExpiresGetProfile_RequiresNewLogin()
    // {
    //     // Step 1: Register and login
    //     // Step 2: GET /api/users/me - works
    //     // Step 3: Wait for token expiration or manually expire
    //     // Step 4: GET /api/users/me - fails (401)
    //     // Step 5: Login again, GET /api/users/me - works
    // }
    //
    // // ===== DATA UPDATE FLOW =====
    //

    [Fact]
    public async Task UserProfileFlow_LoginUpdateProfileGetProfile_ReturnsUpdatedData()
    {
        // Step 1: Register and login
        const string userEmail = "test@garagge.app";
        const string firstName = "John";
        const string lastName = "Doe";
        const string userPassword = "Password123";

        await RegisterAndAuthenticateUser(userEmail, firstName, lastName, userPassword);

        // Step 2: GET /api/users/me - original data
        var getMeResponse = await Client.GetAsync(ApiV1Definition.Users.GetMe);

        getMeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await getMeResponse.Content.ReadFromJsonAsync<UserDto>();

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Email.ShouldBe(userEmail);
        result.FirstName.ShouldBe(firstName);
        result.LastName.ShouldBe(lastName);

        // Step 3: PUT /api/users/me (update profile)
        const string newFirstName = "Jane";
        const string newLastName = "Smith";
        const string newEmail = "new@garagge.app";
        var updateRequest = new UpdateMeCommand(newEmail, newFirstName, newLastName);
        var updateResponse = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<UserDto>();
        updateResult.ShouldNotBeNull();
        updateResult.Id.ShouldBe(result.Id);
        updateResult.Email.ShouldBe(newEmail);
        updateResult.FirstName.ShouldBe(newFirstName);
        updateResult.LastName.ShouldBe(newLastName);

        // Step 4: GET /api/users/me - updated data
        var getMeUpdatedResponse = await Client.GetAsync(ApiV1Definition.Users.GetMe);

        getMeUpdatedResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedResult = await getMeUpdatedResponse.Content.ReadFromJsonAsync<UserDto>();

        updatedResult.ShouldNotBeNull();
        updatedResult.Id.ShouldNotBe(Guid.Empty);
        updatedResult.Email.ShouldBe(newEmail);
        updatedResult.FirstName.ShouldBe(newFirstName);
        updatedResult.LastName.ShouldBe(newLastName);

        // Assert: Data reflects changes
        var user = DbContext.Users.FirstOrDefault();

        user.ShouldNotBeNull();
        user.Id.ShouldBe(result.Id);
        user.Email.ShouldBe(newEmail);
        user.FirstName.ShouldBe(newFirstName);
        user.LastName.ShouldBe(newLastName);
    }


    [Fact]
    public async Task UserProfileFlow_UpdateProfileInvalidDataGetProfile_ProfileUnchanged()
    {
        // Step 1: Register and login
        const string userEmail = "test@garagge.app";
        const string firstName = "John";
        const string lastName = "Doe";
        const string userPassword = "Password123";

        await RegisterAndAuthenticateUser(userEmail, firstName, lastName, userPassword);

        // Step 2: GET /api/users/me - original data
        var getMeResponse = await Client.GetAsync(ApiV1Definition.Users.GetMe);

        getMeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await getMeResponse.Content.ReadFromJsonAsync<UserDto>();

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Email.ShouldBe(userEmail);
        result.FirstName.ShouldBe(firstName);
        result.LastName.ShouldBe(lastName);

        // Step 3: PUT /api/users/me with invalid data (should fail)
        var updateRequest = new UpdateMeCommand("invalid-email", string.Empty, "Smith");

        var updateResponse = await Client.PutAsJsonAsync(ApiV1Definition.Users.UpdateMe, updateRequest);

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Step 4: GET /api/users/me - original data unchanged
        var getMeNotUpdatedResponse = await Client.GetAsync(ApiV1Definition.Users.GetMe);

        getMeNotUpdatedResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var notUpdatedResult = await getMeNotUpdatedResponse.Content.ReadFromJsonAsync<UserDto>();

        notUpdatedResult.ShouldNotBeNull();
        notUpdatedResult.Id.ShouldBe(result.Id);
        notUpdatedResult.Email.ShouldBe(userEmail);
        notUpdatedResult.FirstName.ShouldBe(firstName);
        notUpdatedResult.LastName.ShouldBe(lastName);
    }

    // // ===== MULTI-USER SCENARIOS =====
    //
    // [Fact]
    // public async Task UserProfileFlow_TwoUsersLoginEachGetOwnProfile_DataIsolation()
    // {
    //     // Step 1: Register user A and user B
    //     // Step 2: Login user A, GET /api/users/me
    //     // Step 3: Login user B, GET /api/users/me
    //     // Assert: Each user sees only their own data
    // }
    //
    // [Fact]
    // public async Task UserProfileFlow_UserALoginUserBTokenGetProfile_Unauthorized()
    // {
    //     // Step 1: Register user A and user B
    //     // Step 2: Login user A, get tokenA
    //     // Step 3: Login user B, get tokenB
    //     // Step 4: Use tokenB to GET /api/users/me
    //     // Step 5: Try to use tokenA with different request (security test)
    //     // Assert: Each token only works for its owner
    // }
    //
    // // ===== ERROR RECOVERY FLOW =====
    //
    // [Fact]
    // public async Task UserProfileFlow_LoginGetProfileServerErrorRetry_EventuallySucceeds()
    // {
    //     // Step 1: Register and login
    //     // Step 2: Simulate server error scenario
    //     // Step 3: Retry GET /api/users/me
    //     // Assert: Eventually succeeds with correct data
    // }
    //
    // [Fact]
    // public async Task UserProfileFlow_LoginGetProfileNetworkTimeoutRetry_Succeeds()
    // {
    //     // Step 1: Register and login
    //     // Step 2: First request times out
    //     // Step 3: Retry GET /api/users/me
    //     // Assert: Second request succeeds
    // }
    //
    // // ===== ACCOUNT LIFECYCLE FLOW =====
    //
    // [Fact]
    // public async Task UserProfileFlow_RegisterLoginGetProfileDeleteAccountGetProfile_Fails()
    // {
    //     // Step 1: Register and login
    //     // Step 2: GET /api/users/me - works
    //     // Step 3: DELETE /api/users/me (delete account)
    //     // Step 4: GET /api/users/me - should fail
    // }
    //
    // [Fact]
    // public async Task UserProfileFlow_RegisterLoginGetProfileDeactivateAccountGetProfile_Fails()
    // {
    //     // Step 1: Register and login
    //     // Step 2: GET /api/users/me - works
    //     // Step 3: Deactivate account (if feature exists)
    //     // Step 4: GET /api/users/me - should fail or return limited data
    // }
    //
    // // ===== SECURITY FLOW =====
    //
    // [Fact]
    // public async Task UserProfileFlow_LoginGetProfileChangePasswordOldTokenGetProfile_TokenStillValid()
    // {
    //     // Step 1: Register and login, get token
    //     // Step 2: GET /api/users/me - works
    //     // Step 3: Change password
    //     // Step 4: GET /api/users/me with old token
    //     // Assert: Depends on security policy - might still work or fail
    // }
    //
    // [Fact]
    // public async Task UserProfileFlow_SuspiciousActivityGetProfile_SecurityMeasures()
    // {
    //     // Step 1: Register and login
    //     // Step 2: Simulate suspicious activity
    //     // Step 3: GET /api/users/me
    //     // Assert: May require additional verification or be blocked
    // }
}