using Application.Services;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Vehicles;
using Domain.Enums;

namespace ApplicationTests.Services;

public class EnergyEntryFilterServiceTests
{
    private readonly EnergyEntryFilterService _service;

    public EnergyEntryFilterServiceTests()
    {
        _service = new EnergyEntryFilterService();
    }

    #region ApplyEnergyTypeFilter Tests

    [Fact]
    public void ApplyEnergyTypeFilter_WhenEnergyTypeIsNull_ReturnsUnfilteredQuery()
    {
        // Arrange
        var entries = CreateTestEnergyEntries();
        var query = entries.AsQueryable();

        // Act
        var result = _service.ApplyEnergyTypeFilter(query, null);

        // Assert
        result.Count().ShouldBe(5);
    }
    
    [Fact]
    public void ApplyEnergyTypeFilter_WithEmptyList_ReturnsUnfilteredQuery()
    {
        // Arrange
        var entries = CreateTestEnergyEntries();
        var query = entries.AsQueryable();

        // Act
        var result = _service.ApplyEnergyTypeFilter(query, []);

        // Assert
        result.Count().ShouldBe(entries.Count);
    }
    
    [Fact]
    public void ApplyEnergyTypeFilter_WithMultipleTypes_ReturnsOnlyMatchingEntries()
    {
        // Arrange
        var entries = CreateTestEnergyEntries();
        var query = entries.AsQueryable();
        var types = new[] { EnergyType.Gasoline, EnergyType.Diesel };

        // Act
        var result = _service.ApplyEnergyTypeFilter(query, types);

        // Assert
        var filteredEntries = result.ToList();
        filteredEntries.Count.ShouldBe(3);
        filteredEntries.All(e => types.Contains(e.Type)).ShouldBeTrue();
    }
    
    [Fact]
    public void ApplyEnergyTypeFilter_WhenSingleTypeMatches_ReturnsOnlyMatchingEntries()
    {
        // Arrange
        var entries = CreateTestEnergyEntries();
        var query = entries.AsQueryable();
        var types = new[] { EnergyType.Gasoline };

        // Act
        var result = _service.ApplyEnergyTypeFilter(query, types);

        // Assert
        var filteredEntries = result.ToList();
        filteredEntries.All(e => e.Type == EnergyType.Gasoline).ShouldBeTrue();
    }

    [Fact]
    public void ApplyEnergyTypeFilter_WhenSingleTypeNoMatch_ReturnsEmpty()
    {
        // Arrange
        var entries = CreateTestEnergyEntries();
        var query = entries.AsQueryable();
        var types = new[] { EnergyType.Hydrogen }; // brak w testowych danych

        // Act
        var result = _service.ApplyEnergyTypeFilter(query, types);

        // Assert
        result.Count().ShouldBe(0);
    }

    [Fact]
    public void ApplyEnergyTypeFilter_WhenMultipleTypes_ReturnsOnlyMatchingEntries()
    {
        // Arrange
        var entries = CreateTestEnergyEntries();
        var query = entries.AsQueryable();
        var types = new[] { EnergyType.Gasoline, EnergyType.Diesel };

        // Act
        var result = _service.ApplyEnergyTypeFilter(query, types);

        // Assert
        var filteredEntries = result.ToList();
        filteredEntries.All(e => types.Contains(e.Type)).ShouldBeTrue();
    }

    [Fact]
    public void ApplyEnergyTypeFilter_WhenMultipleTypesWithNoMatch_ReturnsEmpty()
    {
        // Arrange
        var entries = CreateTestEnergyEntries();
        var query = entries.AsQueryable();
        var types = new[] { EnergyType.Hydrogen, EnergyType.Biofuel }; // brak w danych

        // Act
        var result = _service.ApplyEnergyTypeFilter(query, types);

        // Assert
        result.Count().ShouldBe(0);
    }

    #endregion

    #region ApplyUserFilter Tests

    [Fact]
    public void ApplyUserFilter_WithValidUserId_ReturnsOnlyUserEntries()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var entries = CreateTestEnergyEntriesWithUsers(userId, otherUserId);
        var query = entries.AsQueryable();

        // Act
        var result = _service.ApplyUserFilter(query, userId);

        // Assert
        var filteredEntries = result.ToList();
        filteredEntries.Count.ShouldBe(3);
        filteredEntries.All(e => e.Vehicle!.UserId == userId).ShouldBeTrue();
    }

    [Fact]
    public void ApplyUserFilter_WithNonExistentUserId_ReturnsEmptyQuery()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var nonExistentUserId = Guid.NewGuid();
        var entries = CreateTestEnergyEntriesWithUsers(userId, otherUserId);
        var query = entries.AsQueryable();

        // Act
        var result = _service.ApplyUserFilter(query, nonExistentUserId);

        // Assert
        result.Count().ShouldBe(0);
    }

    #endregion

    #region ApplyVehicleFilter Tests

    [Fact]
    public void ApplyVehicleFilter_WithValidVehicleId_ReturnsOnlyVehicleEntries()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var otherVehicleId = Guid.NewGuid();
        var entries = CreateTestEnergyEntriesWithVehicles(vehicleId, otherVehicleId);
        var query = entries.AsQueryable();

        // Act
        var result = _service.ApplyVehicleFilter(query, vehicleId);

        // Assert
        var filteredEntries = result.ToList();
        filteredEntries.Count.ShouldBe(3);
        filteredEntries.All(e => e.VehicleId == vehicleId).ShouldBeTrue();
    }

    [Fact]
    public void ApplyVehicleFilter_WithNonExistentVehicleId_ReturnsEmptyQuery()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        var otherVehicleId = Guid.NewGuid();
        var nonExistentVehicleId = Guid.NewGuid();
        var entries = CreateTestEnergyEntriesWithVehicles(vehicleId, otherVehicleId);
        var query = entries.AsQueryable();

        // Act
        var result = _service.ApplyVehicleFilter(query, nonExistentVehicleId);

        // Assert
        result.Count().ShouldBe(0);
    }

    #endregion

    #region ApplyDefaultSorting Tests

    [Fact]
    public void ApplyDefaultSorting_SortsByDateDescendingThenMileageDescending()
    {
        // Arrange
        var entries = CreateTestEnergyEntriesWithDifferentDatesAndMileage();
        var query = entries.AsQueryable();

        // Act
        var result = _service.ApplyDefaultSorting(query);

        // Assert
        var sortedEntries = result.ToList();
        sortedEntries.Count.ShouldBe(4);
        
        // Check sorting: Date DESC, then Mileage DESC
        sortedEntries[0].Date.ShouldBe(new DateOnly(2023, 10, 16)); // Latest date
        sortedEntries[1].Date.ShouldBe(new DateOnly(2023, 10, 15)); // Same date, higher mileage
        sortedEntries[1].Mileage.ShouldBe(1100);
        sortedEntries[2].Date.ShouldBe(new DateOnly(2023, 10, 15)); // Same date, lower mileage
        sortedEntries[2].Mileage.ShouldBe(1000);
        sortedEntries[3].Date.ShouldBe(new DateOnly(2023, 10, 10)); // Earliest date
    }

    #endregion

    #region Helper Methods

    private static List<EnergyEntry> CreateTestEnergyEntries()
    {
        return new List<EnergyEntry>
        {
            new() { Id = Guid.NewGuid(), Type = EnergyType.Gasoline, VehicleId = Guid.NewGuid(), Date = new DateOnly(2023, 10, 1), Mileage = 1000, EnergyUnit = EnergyUnit.Liter, Volume = 50m },
            new() { Id = Guid.NewGuid(), Type = EnergyType.Gasoline, VehicleId = Guid.NewGuid(), Date = new DateOnly(2023, 10, 2), Mileage = 1100, EnergyUnit = EnergyUnit.Liter, Volume = 45m },
            new() { Id = Guid.NewGuid(), Type = EnergyType.Diesel, VehicleId = Guid.NewGuid(), Date = new DateOnly(2023, 10, 3), Mileage = 2000, EnergyUnit = EnergyUnit.Liter, Volume = 55m },
            new() { Id = Guid.NewGuid(), Type = EnergyType.Electric, VehicleId = Guid.NewGuid(), Date = new DateOnly(2023, 10, 4), Mileage = 3000, EnergyUnit = EnergyUnit.kWh, Volume = 30m },
            new() { Id = Guid.NewGuid(), Type = EnergyType.LPG, VehicleId = Guid.NewGuid(), Date = new DateOnly(2023, 10, 5), Mileage = 1500, EnergyUnit = EnergyUnit.Liter, Volume = 40m }
        };
    }

    private static List<EnergyEntry> CreateTestEnergyEntriesWithUsers(Guid userId, Guid otherUserId)
    {
        var vehicle1 = new Vehicle { Id = Guid.NewGuid(), UserId = userId, Brand = "Toyota", Model = "Corolla", EngineType = EngineType.Fuel };
        var vehicle2 = new Vehicle { Id = Guid.NewGuid(), UserId = otherUserId, Brand = "Honda", Model = "Civic", EngineType = EngineType.Fuel };

        return new List<EnergyEntry>
        {
            new() { Id = Guid.NewGuid(), VehicleId = vehicle1.Id, Vehicle = vehicle1, Type = EnergyType.Gasoline, Date = new DateOnly(2023, 10, 1), Mileage = 1000, EnergyUnit = EnergyUnit.Liter, Volume = 50m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicle1.Id, Vehicle = vehicle1, Type = EnergyType.Gasoline, Date = new DateOnly(2023, 10, 2), Mileage = 1100, EnergyUnit = EnergyUnit.Liter, Volume = 45m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicle1.Id, Vehicle = vehicle1, Type = EnergyType.Diesel, Date = new DateOnly(2023, 10, 3), Mileage = 1200, EnergyUnit = EnergyUnit.Liter, Volume = 55m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicle2.Id, Vehicle = vehicle2, Type = EnergyType.Electric, Date = new DateOnly(2023, 10, 4), Mileage = 2000, EnergyUnit = EnergyUnit.kWh, Volume = 30m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicle2.Id, Vehicle = vehicle2, Type = EnergyType.LPG, Date = new DateOnly(2023, 10, 5), Mileage = 2100, EnergyUnit = EnergyUnit.Liter, Volume = 40m }
        };
    }

    private static List<EnergyEntry> CreateTestEnergyEntriesWithVehicles(Guid vehicleId, Guid otherVehicleId)
    {
        return new List<EnergyEntry>
        {
            new() { Id = Guid.NewGuid(), VehicleId = vehicleId, Type = EnergyType.Gasoline, Date = new DateOnly(2023, 10, 1), Mileage = 1000, EnergyUnit = EnergyUnit.Liter, Volume = 50m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicleId, Type = EnergyType.Diesel, Date = new DateOnly(2023, 10, 2), Mileage = 1100, EnergyUnit = EnergyUnit.Liter, Volume = 45m },
            new() { Id = Guid.NewGuid(), VehicleId = vehicleId, Type = EnergyType.Electric, Date = new DateOnly(2023, 10, 3), Mileage = 1200, EnergyUnit = EnergyUnit.kWh, Volume = 30m },
            new() { Id = Guid.NewGuid(), VehicleId = otherVehicleId, Type = EnergyType.LPG, Date = new DateOnly(2023, 10, 4), Mileage = 2000, EnergyUnit = EnergyUnit.Liter, Volume = 40m },
            new() { Id = Guid.NewGuid(), VehicleId = otherVehicleId, Type = EnergyType.Gasoline, Date = new DateOnly(2023, 10, 5), Mileage = 2100, EnergyUnit = EnergyUnit.Liter, Volume = 55m }
        };
    }

    private static List<EnergyEntry> CreateTestEnergyEntriesWithDifferentDatesAndMileage()
    {
        return new List<EnergyEntry>
        {
            new() { Id = Guid.NewGuid(), Type = EnergyType.Gasoline, VehicleId = Guid.NewGuid(), Date = new DateOnly(2023, 10, 15), Mileage = 1000, EnergyUnit = EnergyUnit.Liter, Volume = 50m },
            new() { Id = Guid.NewGuid(), Type = EnergyType.Diesel, VehicleId = Guid.NewGuid(), Date = new DateOnly(2023, 10, 15), Mileage = 1100, EnergyUnit = EnergyUnit.Liter, Volume = 45m },
            new() { Id = Guid.NewGuid(), Type = EnergyType.Electric, VehicleId = Guid.NewGuid(), Date = new DateOnly(2023, 10, 16), Mileage = 950, EnergyUnit = EnergyUnit.kWh, Volume = 30m },
            new() { Id = Guid.NewGuid(), Type = EnergyType.LPG, VehicleId = Guid.NewGuid(), Date = new DateOnly(2023, 10, 10), Mileage = 2000, EnergyUnit = EnergyUnit.Liter, Volume = 40m }
        };
    }

    #endregion
}
