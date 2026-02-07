using Application.EnergyEntries;
using Application.EnergyEntries.GetByVehicle;
using Application.Services;
using Application.Vehicles;
using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace ApplicationTests.EnergyEntries.GetEnergyEntriesByVehicle;

public class GetEnergyEntriesByVehicleQueryHandlerTests : InMemoryDbTestBase
{
    private readonly GetEnergyEntriesByVehicleQueryHandler _handler;

    public GetEnergyEntriesByVehicleQueryHandlerTests()
    {
        var filterService = new EnergyEntryFilterService();
        _handler = new GetEnergyEntriesByVehicleQueryHandler(Context, UserContextMock.Object, filterService);
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsVehicleNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var nonExistentVehicleId = Guid.NewGuid();
        var query = new GetEnergyEntriesByVehicleQuery(nonExistentVehicleId, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound);
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline], otherUserId);
        var query = new GetEnergyEntriesByVehicleQuery(vehicle.Id, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.NotFound);
    }

    [Fact]
    public async Task Handle_VehicleHasNoEnergyEntries_CallsFilterServiceCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var query = new GetEnergyEntriesByVehicleQuery(vehicle.Id, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
        result.Value.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WithEnergyTypeFilter_CallsFilterServiceWithCorrectParameters()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Electric]);
        var query = new GetEnergyEntriesByVehicleQuery(vehicle.Id, 1, 20, [EnergyType.Electric]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsAllFilterMethodsInCorrectOrder()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var query = new GetEnergyEntriesByVehicleQuery(vehicle.Id, 3, 5, [EnergyType.Gasoline]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Page.ShouldBe(3);
        result.Value.PageSize.ShouldBe(5);
    }

    [Fact]
    public async Task Handle_VehicleHasEnergyEntries_ReturnsPagedListWithEntries()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var entry1 = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline, new DateOnly(2023, 10, 15), 1000);
        var entry2 = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline, new DateOnly(2023, 10, 10), 950);

        var query = new GetEnergyEntriesByVehicleQuery(vehicle.Id, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Items[0].Id.ShouldBe(entry1.Id); // Newer entry first (sorted by date desc)
        result.Value.Items[1].Id.ShouldBe(entry2.Id);
    }

    [Fact]
    public async Task Handle_WithFlagsEnergyTypeFilter_ReturnsMatchingEntries()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline, EnergyType.Diesel, EnergyType.Electric]);

        _ = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline, new DateOnly(2023, 10, 15), 1000);
        _ = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Diesel, new DateOnly(2023, 10, 14), 950);
        _ = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Electric, new DateOnly(2023, 10, 13), 900);

        var query = new GetEnergyEntriesByVehicleQuery(vehicle.Id, 1, 20, [EnergyType.Gasoline, EnergyType.Diesel]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);

        var returnedTypes = result.Value.Items.Select(x => x.Type).ToList();
        returnedTypes.ShouldContain(EnergyType.Gasoline);
        returnedTypes.ShouldContain(EnergyType.Diesel);
        returnedTypes.ShouldNotContain(EnergyType.Electric);
    }

    private async Task<EnergyEntry> CreateEnergyEntryInDb(Guid vehicleId, EnergyType energyType, DateOnly date, int mileage)
    {
        var energyEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            Vehicle = null!,
            Date = date,
            Mileage = mileage,
            Type = energyType,
            EnergyUnit = energyType == EnergyType.Electric ? EnergyUnit.kWh : EnergyUnit.Liter,
            Volume = 50m,
            Cost = 100m,
            PricePerUnit = 2m
        };

        Context.EnergyEntries.Add(energyEntry);
        await Context.SaveChangesAsync();
        return energyEntry;
    }
}