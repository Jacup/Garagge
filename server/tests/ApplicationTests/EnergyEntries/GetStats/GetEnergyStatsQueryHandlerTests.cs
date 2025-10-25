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

    public GetEnergyStatsQueryHandlerTests()
    {
        IEnergyStatsService energyStatsService = new EnergyStatsService();
        _handler = new GetEnergyStatsQueryHandler(Context, UserContextMock.Object, energyStatsService);
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
    public async Task Handle_VehicleHasNoEnergyEntries_CallsFilterServiceCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.EnergyTypes.ShouldBeEmpty();
        result.Value.TotalVolume.ShouldBe(0);
        result.Value.AverageConsumption.ShouldBe(0);
        result.Value.TotalCost.ShouldBe(0);
        result.Value.AveragePricePerUnit.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_VehicleHasOneEnergyType_CalculatesStatsCorrectly()
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
            Cost = 100,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id
        };
        var entry2 = new EnergyEntry()
        {
            Date = new DateOnly(2020, 01, 16),
            Mileage = 1500,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 60,
            Cost = 150,
            PricePerUnit = 10m,
            VehicleId = vehicle.Id
        };

        Context.EnergyEntries.AddRange(entry1, entry2);
        await Context.SaveChangesAsync();

        var query = new GetEnergyStatsQuery(vehicle.Id, []);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        result.Value.TotalVolume.ShouldBe(100);
        result.Value.AverageConsumption.ShouldBe(12m); // (60/500)*100 = 12 l/100km
        result.Value.TotalCost.ShouldBe(250);
        result.Value.AveragePricePerUnit.ShouldBe(7.5m);
    }

    [Fact]
    public async Task Handle_VehicleHasMultipleEnergyTypes_CalculatesStatsCorrectly()
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
        result.Value.EnergyTypes.Length.ShouldBe(2);
        result.Value.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        result.Value.EnergyTypes.ShouldContain(EnergyType.Diesel);
        result.Value.TotalVolume.ShouldBe(100);
        result.Value.AverageConsumption.ShouldBe(12m); // (60/500)*100 = 12 l/100km
        result.Value.TotalCost.ShouldBe(440);
        result.Value.AveragePricePerUnit.ShouldBe(4.5m);
    }

    [Fact]
    public async Task Handle_FilterByEnergyType_ReturnsFilteredStats()
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

        var query = new GetEnergyStatsQuery(vehicle.Id, [EnergyType.Gasoline]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.EnergyTypes.Length.ShouldBe(1);
        result.Value.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        result.Value.TotalVolume.ShouldBe(40);
        result.Value.AverageConsumption.ShouldBe(0);
        result.Value.TotalCost.ShouldBe(200);
        result.Value.AveragePricePerUnit.ShouldBe(5m);
    }

    [Fact]
    public async Task Handle_SingleEnergyType_FilterBySameType_ReturnsFilteredStats()
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

        var query = new GetEnergyStatsQuery(vehicle.Id, [EnergyType.Gasoline]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.EnergyTypes.Length.ShouldBe(1);
        result.Value.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        result.Value.TotalVolume.ShouldBe(100);
        result.Value.AverageConsumption.ShouldBe(12m); // (60/500)*100 = 12 l/100km
        result.Value.TotalCost.ShouldBe(500);
        result.Value.AveragePricePerUnit.ShouldBe(5m);
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
        result.Value.EnergyTypes.Length.ShouldBe(2);
        result.Value.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        result.Value.EnergyTypes.ShouldContain(EnergyType.Diesel);
        result.Value.TotalVolume.ShouldBe(100);
        result.Value.AverageConsumption.ShouldBe(12m); // (60/500)*100 = 12 l/100km
        result.Value.TotalCost.ShouldBe(440);
        result.Value.AveragePricePerUnit.ShouldBe(4.5m);
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
        result.Value.TotalVolume.ShouldBe(100);
        result.Value.AverageConsumption.ShouldBe(12m);
        result.Value.TotalCost.ShouldBe(300);
        result.Value.AveragePricePerUnit.ShouldBe(5m); // Only counts non-null prices
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
        result.Value.TotalVolume.ShouldBe(115);
        // Consumption 1: (30/500)*100 = 6 l/100km
        // Consumption 2: (35/500)*100 = 7 l/100km
        // Average: (6+7)/2 = 6.5 l/100km
        result.Value.AverageConsumption.ShouldBe(6.5m);
        result.Value.TotalCost.ShouldBe(575);
        result.Value.AveragePricePerUnit.ShouldBe(5m);
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
        result.Value.EnergyTypes.ShouldBeEmpty();
        result.Value.TotalVolume.ShouldBe(0);
        result.Value.AverageConsumption.ShouldBe(0);
        result.Value.TotalCost.ShouldBe(0);
        result.Value.AveragePricePerUnit.ShouldBe(0);
    }
}