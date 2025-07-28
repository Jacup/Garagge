using Application.Abstractions.Authentication;
using Infrastructure.Authentication;

namespace InfrastructureTests.Authentication;

public class PasswordHasherTests
{
    private readonly IPasswordHasher _hasher = new PasswordHasher();

    [Fact]
    public void Hash_PlainPassword_ReturnsHashWithSaltInCorrectFormat()
    {
        // Arrange
        var password = "SecureP@ss123";

        // Act
        var result = _hasher.Hash(password);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        result.Split('-').Length.ShouldBe(2);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "correct horse battery staple";
        var hash = _hasher.Hash(password);

        // Act
        var result = _hasher.Verify(password, hash);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Verify_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "original";
        var hash = _hasher.Hash(password);

        // Act
        var result = _hasher.Verify("wrong-password", hash);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Arrange
        var password = "repeated";

        // Act
        var hash1 = _hasher.Hash(password);
        var hash2 = _hasher.Hash(password);

        // Assert
        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void Verify_TamperedHash_ReturnsFalse()
    {
        // Arrange
        var password = "userpass";
        var hash = _hasher.Hash(password);

        // Act
        var tampered = "0" + hash[1..];
        var result = _hasher.Verify(password, tampered);

        // Assert
        result.ShouldBeFalse();
    }
}
