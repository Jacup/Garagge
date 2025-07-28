using Domain.Entities.Users;

namespace DomainTests.Users;

public class UserTests
{
    [Fact]
    public void Constructor_ValidProperties_CreatesUserWithCorrectData()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var passwordHash = "hashed_password";

        // Act
        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash
        };

        // Assert
        user.Email.ShouldBe(email);
        user.FirstName.ShouldBe(firstName);
        user.LastName.ShouldBe(lastName);
        user.PasswordHash.ShouldBe(passwordHash);
    }

    [Fact]
    public void Setters_PropertiesUpdated_UpdatesUserDataCorrectly()
    {
        // Arrange
        var user = new User
        {
            Email = "initial@example.com",
            FirstName = "Initial",
            LastName = "User",
            PasswordHash = "initial_hash"
        };

        // Act
        user.Email = "updated@example.com";
        user.FirstName = "Updated";
        user.LastName = "Name";
        user.PasswordHash = "new_hash";

        // Assert
        user.Email.ShouldBe("updated@example.com");
        user.FirstName.ShouldBe("Updated");
        user.LastName.ShouldBe("Name");
        user.PasswordHash.ShouldBe("new_hash");
    }
}
