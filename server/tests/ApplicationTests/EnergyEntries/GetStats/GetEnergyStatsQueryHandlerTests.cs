using Application.EnergyEntries;
using Application.EnergyEntries.GetStats;
using Application.Services.EnergyStats;
using Application.Vehicles;
using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace ApplicationTests.EnergyEntries.GetStats;

public class GetEnergyStatsQueryHandlerTests : InMemoryDbTestBase
{
    private readonly GetEnergyStatsQueryHandler _handler;
    private readonly IEnergyStatsService _energyStatsService;

    public GetEnergyStatsQueryHandlerTests()
    {
        _energyStatsService = new EnergyStatsService();
        _handler = new GetEnergyStatsQueryHandler(Context, UserContextMock.Object, _energyStatsService);
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsVehicleNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var nonExistentVehicleId = Guid.NewGuid();
        var query = new GetEnergyStatsQuery(nonExistentVehicleId, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(nonExistentVehicleId));
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsUnauthorizedError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline], otherUserId);
        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_VehicleHasNoEnergyEntries_ReturnsEmptyStats()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.VehicleId.ShouldBe(vehicle.Id);
        result.Value.TotalCost.ShouldBe(0);
        result.Value.TotalEntries.ShouldBe(0);
        result.Value.EnergyUnitStats.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_SingleUnitSingleType_CalculatesStatsCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);

        var entry1 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        var entry2 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 17),
            Mileage = 1500,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 60,
            Cost = 300,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.AddRange(entry1, entry2);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.VehicleId.ShouldBe(vehicle.Id);
        result.Value.TotalCost.ShouldBe(500);
        result.Value.TotalEntries.ShouldBe(2);
        result.Value.EnergyUnitStats.Length.ShouldBe(1);

        var literStats = result.Value.EnergyUnitStats[0];
        literStats.Unit.ShouldBe(EnergyUnit.Liter);
        literStats.EntriesCount.ShouldBe(2);
        literStats.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        literStats.TotalVolume.ShouldBe(100);
        literStats.TotalCost.ShouldBe(500);
        literStats.AverageConsumption.ShouldBe(12m); // (60/500)*100 = 12 l/100km
        literStats.AveragePricePerUnit.ShouldBe(5m);
        literStats.AverageCostPer100km.ShouldBe(0.6m); // (12/100)*5 = 0.6
    }

    [Fact]
    public async Task Handle_SingleUnitMultipleTypes_GroupsByUnit()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline, EnergyType.Diesel]);

        var entry1 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        var entry2 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 17),
            Mileage = 1500,
            Type = EnergyType.Diesel,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 60,
            Cost = 240,
            PricePerUnit = 4m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.AddRange(entry1, entry2);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.VehicleId.ShouldBe(vehicle.Id);
        result.Value.TotalCost.ShouldBe(440);
        result.Value.TotalEntries.ShouldBe(2);
        result.Value.EnergyUnitStats.Length.ShouldBe(1);

        var literStats = result.Value.EnergyUnitStats[0];
        literStats.Unit.ShouldBe(EnergyUnit.Liter);
        literStats.EntriesCount.ShouldBe(2);
        literStats.EnergyTypes.Count.ShouldBe(2);
        literStats.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        literStats.EnergyTypes.ShouldContain(EnergyType.Diesel);
        literStats.TotalVolume.ShouldBe(100);
        literStats.TotalCost.ShouldBe(440);
        literStats.AverageConsumption.ShouldBe(12m); // (60/500)*100 = 12 l/100km
        literStats.AveragePricePerUnit.ShouldBe(4.5m);
    }

    [Fact]
    public async Task Handle_MultipleUnits_GroupsCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline, EnergyType.Electric]);

        var entry1 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        var entry2 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 17),
            Mileage = 1500,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 60,
            Cost = 300,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        var entry3 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 18),
            Mileage = 2000,
            Type = EnergyType.Electric,
            EnergyUnit = EnergyUnit.kWh,
            Volume = 50,
            Cost = 100,
            PricePerUnit = 2m,
            VehicleId = vehicle.Id
        };
        var entry4 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 19),
            Mileage = 2500,
            Type = EnergyType.Electric,
            EnergyUnit = EnergyUnit.kWh,
            Volume = 60,
            Cost = 120,
            PricePerUnit = 2m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.AddRange(entry1, entry2, entry3, entry4);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.VehicleId.ShouldBe(vehicle.Id);
        result.Value.TotalCost.ShouldBe(720);
        result.Value.TotalEntries.ShouldBe(4);
        result.Value.EnergyUnitStats.Length.ShouldBe(2);

        var literStats = result.Value.EnergyUnitStats.First(s => s.Unit == EnergyUnit.Liter);
        literStats.EntriesCount.ShouldBe(2);
        literStats.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        literStats.TotalVolume.ShouldBe(100);
        literStats.TotalCost.ShouldBe(500);
        literStats.AverageConsumption.ShouldBe(12m);
        literStats.AveragePricePerUnit.ShouldBe(5m);

        var kWhStats = result.Value.EnergyUnitStats.First(s => s.Unit == EnergyUnit.kWh);
        kWhStats.EntriesCount.ShouldBe(2);
        kWhStats.EnergyTypes.ShouldContain(EnergyType.Electric);
        kWhStats.TotalVolume.ShouldBe(110);
        kWhStats.TotalCost.ShouldBe(220);
        kWhStats.AverageConsumption.ShouldBe(12m); // (60/500)*100 = 12 kWh/100km
        kWhStats.AveragePricePerUnit.ShouldBe(2m);
    }

    [Fact]
    public async Task Handle_FilterBySingleEnergyType_ReturnsFilteredStats()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline, EnergyType.Diesel]);

        var entry1 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        var entry2 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 17),
            Mileage = 1500,
            Type = EnergyType.Diesel,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 60,
            Cost = 240,
            PricePerUnit = 4m,
            VehicleId = vehicle.Id
        };
        var entry3 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 18),
            Mileage = 2000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 50,
            Cost = 250,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.AddRange(entry1, entry2, entry3);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, [EnergyType.Gasoline]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(450); // Only Gasoline entries
        result.Value.TotalEntries.ShouldBe(2);
        result.Value.EnergyUnitStats.Length.ShouldBe(1);

        var literStats = result.Value.EnergyUnitStats[0];
        literStats.Unit.ShouldBe(EnergyUnit.Liter);
        literStats.EntriesCount.ShouldBe(2);
        literStats.EnergyTypes.Count.ShouldBe(1);
        literStats.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        literStats.TotalVolume.ShouldBe(90);
        literStats.AverageConsumption.ShouldBe(5m); // (50/1000)*100 = 5 l/100km
    }

    [Fact]
    public async Task Handle_FilterByMultipleEnergyTypes_ReturnsFilteredStats()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline, EnergyType.Diesel, EnergyType.Electric]);

        var entry1 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        var entry2 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 17),
            Mileage = 1500,
            Type = EnergyType.Diesel,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 60,
            Cost = 240,
            PricePerUnit = 4m,
            VehicleId = vehicle.Id
        };
        var entry3 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 18),
            Mileage = 2000,
            Type = EnergyType.Electric,
            EnergyUnit = EnergyUnit.kWh,
            Volume = 50,
            Cost = 100,
            PricePerUnit = 2m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.AddRange(entry1, entry2, entry3);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, [EnergyType.Gasoline, EnergyType.Diesel]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(440); // Excludes Electric
        result.Value.TotalEntries.ShouldBe(2);
        result.Value.EnergyUnitStats.Length.ShouldBe(1);

        var literStats = result.Value.EnergyUnitStats[0];
        literStats.Unit.ShouldBe(EnergyUnit.Liter);
        literStats.EnergyTypes.Count.ShouldBe(2);
        literStats.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        literStats.EnergyTypes.ShouldContain(EnergyType.Diesel);
    }

    [Fact]
    public async Task Handle_FilterReturnsNoEntries_ReturnsEmptyStats()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);

        var entry1 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.Add(entry1);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, [EnergyType.Diesel]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(0);
        result.Value.TotalEntries.ShouldBe(0);
        result.Value.EnergyUnitStats.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_EntriesWithNullCost_CalculatesCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);

        var entry1 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = null,
            PricePerUnit = null,
            VehicleId = vehicle.Id
        };
        var entry2 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 17),
            Mileage = 1500,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 60,
            Cost = 300,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.AddRange(entry1, entry2);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(300); // Only counts non-null costs

        var literStats = result.Value.EnergyUnitStats[0];
        literStats.TotalVolume.ShouldBe(100);
        literStats.TotalCost.ShouldBe(300);
        literStats.AveragePricePerUnit.ShouldBe(5m); // Only counts non-null prices
        literStats.AverageCostPer100km.ShouldBe(0.6m); // (12/100)*5
    }

    [Fact]
    public async Task Handle_SingleEntry_ReturnsZeroConsumption()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);

        var entry1 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.Add(entry1);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalEntries.ShouldBe(1);

        var literStats = result.Value.EnergyUnitStats[0];
        literStats.EntriesCount.ShouldBe(1);
        literStats.AverageConsumption.ShouldBe(0); // Can't calculate with single entry
        literStats.AverageCostPer100km.ShouldBe(0); // No consumption = no cost per 100km
    }

    [Fact]
    public async Task Handle_MultipleEntriesForConsumption_CalculatesAverageCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);

        var entry1 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 50,
            Cost = 250,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        var entry2 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 17),
            Mileage = 1500,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 30,
            Cost = 150,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        var entry3 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 18),
            Mileage = 2000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 35,
            Cost = 175,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.AddRange(entry1, entry2, entry3);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(575);
        result.Value.TotalEntries.ShouldBe(3);

        var literStats = result.Value.EnergyUnitStats[0];
        literStats.TotalVolume.ShouldBe(115);
        // Consumption 1: (30/500)*100 = 6 l/100km
        // Consumption 2: (35/500)*100 = 7 l/100km
        // Average: (6+7)/2 = 6.5 l/100km
        literStats.AverageConsumption.ShouldBe(6.5m);
        literStats.AveragePricePerUnit.ShouldBe(5m);
        literStats.AverageCostPer100km.ShouldBe(0.325m); // (6.5/100)*5
    }

    [Fact]
    public async Task Handle_MixedUnitsAndTypes_CalculatesSeparately()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline, EnergyType.Diesel]);

        // Gasoline in Liters
        var entry1 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        var entry2 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 17),
            Mileage = 1500,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 60,
            Cost = 300,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        // Diesel in Gallons
        var entry3 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 18),
            Mileage = 2000,
            Type = EnergyType.Diesel,
            EnergyUnit = EnergyUnit.Gallon,
            Volume = 10,
            Cost = 100,
            PricePerUnit = 10m,
            VehicleId = vehicle.Id
        };
        var entry4 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 19),
            Mileage = 2500,
            Type = EnergyType.Diesel,
            EnergyUnit = EnergyUnit.Gallon,
            Volume = 15,
            Cost = 150,
            PricePerUnit = 10m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.AddRange(entry1, entry2, entry3, entry4);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(750);
        result.Value.TotalEntries.ShouldBe(4);
        result.Value.EnergyUnitStats.Length.ShouldBe(2);

        var literStats = result.Value.EnergyUnitStats.First(s => s.Unit == EnergyUnit.Liter);
        literStats.EntriesCount.ShouldBe(2);
        literStats.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        literStats.TotalVolume.ShouldBe(100);
        literStats.AverageConsumption.ShouldBe(12m);

        var gallonStats = result.Value.EnergyUnitStats.First(s => s.Unit == EnergyUnit.Gallon);
        gallonStats.EntriesCount.ShouldBe(2);
        gallonStats.EnergyTypes.ShouldContain(EnergyType.Diesel);
        gallonStats.TotalVolume.ShouldBe(25);
        gallonStats.AverageConsumption.ShouldBe(3m); // (15/500)*100 = 3 gallons/100km
    }
}

