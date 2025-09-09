using Application.Core;
using Application.Vehicles;

namespace ApplicationTests.Vehicles;

public class VehicleErrorTests
{
    [Fact]
    public void Unauthorized_WhenAccessed_ReturnsUnauthorizedError()
    {
        // Act
        var result = VehicleErrors.Unauthorized;
        
        // Assert
        result.Type.ShouldBe(ErrorType.Unauthorized);
        result.Code.ShouldBe("Vehicles.Unauthorized");
        result.Description.ShouldBe("You are not authorized to perform this action.");
    }
    
    [Fact]
    public void CreateFailed_WhenAccessed_ReturnsFailureError()
    {
        // Act
        var result = VehicleErrors.CreateFailed;
        
        // Assert
        result.Type.ShouldBe(ErrorType.Failure);
        result.Code.ShouldBe("Vehicles.CreateFailed");
        result.Description.ShouldBe("Failed to create vehicle");
    }
    
    [Fact]
    public void DeleteFailed_WhenCalledWithVehicleId_ReturnsFailureErrorWithId()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        
        // Act
        var result = VehicleErrors.DeleteFailed(vehicleId);
        
        // Assert
        result.Type.ShouldBe(ErrorType.Failure);
        result.Code.ShouldBe("Vehicles.DeleteFailed");
        result.Description.ShouldBe($"Failed to delete vehicle with Id = '{vehicleId}'");
    }
    
    [Fact]
    public void NotFoundForUser_WhenCalledWithUserId_ReturnsNotFoundErrorWithUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act
        var result = VehicleErrors.NotFoundForUser(userId);
        
        // Assert
        result.Type.ShouldBe(ErrorType.NotFound);
        result.Code.ShouldBe("Vehicles.NotFound");
        result.Description.ShouldBe($"Not found any vehicles with the UserId = '{userId}'");
    }
    
    [Fact]
    public void NotFound_WhenCalledWithVehicleId_ReturnsNotFoundErrorWithVehicleId()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        
        // Act
        var result = VehicleErrors.NotFound(vehicleId);
        
        // Assert
        result.Type.ShouldBe(ErrorType.NotFound);
        result.Code.ShouldBe("Vehicles.NotFound");
        result.Description.ShouldBe($"Not found vehicle with Id = '{vehicleId}'");
    }
}
