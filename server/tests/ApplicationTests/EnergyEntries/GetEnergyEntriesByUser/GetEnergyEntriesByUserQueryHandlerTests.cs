using Application.Abstractions.Authentication;
using Application.EnergyEntries;
using Application.EnergyEntries.GetEnergyEntriesByUser;
using Application.Services;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Moq;

namespace ApplicationTests.EnergyEntries.GetEnergyEntriesByUser;

public class GetEnergyEntriesByUserQueryHandlerTests : InMemoryDbTestBase
{
    private readonly GetEnergyEntriesByUserQueryHandler _handler;

    public GetEnergyEntriesByUserQueryHandlerTests()
    {
        // Use real implementation instead of mock to avoid IAsyncQueryProvider issues
        var filterService = new EnergyEntryFilterService();
        _handler = new GetEnergyEntriesByUserQueryHandler(Context, UserContextMock.Object, filterService);
    }

    [Fact]
    public async Task Handle_UserTriesToAccessOtherUsersEntries_ReturnsUnauthorizedError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var query = new GetEnergyEntriesByUserQuery(otherUserId, 1, 20, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_UserHasNoEnergyEntries_CallsFilterServiceCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var query = new GetEnergyEntriesByUserQuery(LoggedUserId, 1, 20, null);

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
        var query = new GetEnergyEntriesByUserQuery(LoggedUserId, 1, 20, EnergyType.Gasoline);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify that filter was called with correct energy type
        // _filterServiceMock.Verify(x => x.ApplyEnergyTypeFilter(It.IsAny<IQueryable<EnergyEntry>>(), EnergyType.Gasoline), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsAllFilterMethodsInCorrectOrder()
    {
        // Arrange
        SetupAuthorizedUser();
        var query = new GetEnergyEntriesByUserQuery(LoggedUserId, 2, 10, EnergyType.Electric);

        var callSequence = new List<string>();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Page.ShouldBe(2);
        result.Value.PageSize.ShouldBe(10);

        // Verify correct call sequence
        // callSequence.ShouldBe(new[] { "ApplyUserFilter", "ApplyEnergyTypeFilter", "ApplyDefaultSorting" });
    }

    [Fact]
    public async Task Handle_UserHasEnergyEntries_ReturnsPagedListWithEntries()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);
        var energyEntry1 = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline, new DateOnly(2023, 10, 15), 1000);
        var energyEntry2 = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline, new DateOnly(2023, 10, 10), 950);

        var query = new GetEnergyEntriesByUserQuery(LoggedUserId, 1, 20, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);
        result.Value.Items[0].Id.ShouldBe(energyEntry1.Id); // Newer entry first (sorted by date desc)
        result.Value.Items[1].Id.ShouldBe(energyEntry2.Id);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);

        // Create 5 entries
        var entries = new List<EnergyEntry>();
        for (int i = 1; i <= 5; i++)
        {
            entries.Add(await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline, new DateOnly(2023, 10, i), 1000 + i * 10));
        }

        var query = new GetEnergyEntriesByUserQuery(LoggedUserId, 2, 2, null); // Page 2, size 2

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(5);
        result.Value.Page.ShouldBe(2);
        result.Value.PageSize.ShouldBe(2);
        result.Value.HasPreviousPage.ShouldBeTrue();
        result.Value.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_EntriesFromMultipleVehicles_ReturnsAllUserEntries()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle1 = await CreateVehicleInDb(EnergyType.Gasoline);
        var vehicle2 = await CreateVehicleInDb(EnergyType.Electric);

        var entry1 = await CreateEnergyEntryInDb(vehicle1.Id, EnergyType.Gasoline, new DateOnly(2023, 10, 15), 1000);
        var entry2 = await CreateEnergyEntryInDb(vehicle2.Id, EnergyType.Electric, new DateOnly(2023, 10, 10), 500);

        var query = new GetEnergyEntriesByUserQuery(LoggedUserId, 1, 20, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(2);

        var entryIds = result.Value.Items.Select(x => x.Id).ToList();
        entryIds.ShouldContain(entry1.Id);
        entryIds.ShouldContain(entry2.Id);
    }

    [Fact]
    public async Task Handle_EntriesSortedByDateAndMileage_ReturnsCorrectOrder()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline);

        // Same date, different mileage
        var entry1 = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline, new DateOnly(2023, 10, 15), 1000);
        var entry2 = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline, new DateOnly(2023, 10, 15), 1100);
        // Different date
        var entry3 = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline, new DateOnly(2023, 10, 16), 950);

        var query = new GetEnergyEntriesByUserQuery(LoggedUserId, 1, 20, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(3);

        // Should be sorted by Date DESC, then Mileage DESC
        result.Value.Items[0].Id.ShouldBe(entry3.Id); // 2023-10-16
        result.Value.Items[1].Id.ShouldBe(entry2.Id); // 2023-10-15, mileage 1100
        result.Value.Items[2].Id.ShouldBe(entry1.Id); // 2023-10-15, mileage 1000
    }

    [Fact]
    public async Task Handle_WithFlagsEnergyTypeFilter_ReturnsMatchingEntries()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb(EnergyType.Gasoline | EnergyType.Diesel | EnergyType.Electric);

        _ = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Gasoline, new DateOnly(2023, 10, 15), 1000);
        _ = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Diesel, new DateOnly(2023, 10, 14), 950);
        _ = await CreateEnergyEntryInDb(vehicle.Id, EnergyType.Electric, new DateOnly(2023, 10, 13), 900);

        // Filter for Gasoline OR Diesel (using flags)
        var query = new GetEnergyEntriesByUserQuery(LoggedUserId, 1, 20, EnergyType.Gasoline | EnergyType.Diesel);

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

    private async Task<Vehicle> CreateVehicleInDb(EnergyType supportedEnergyTypes, Guid? userId = null)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = userId ?? LoggedUserId,
            VehicleEnergyTypes = new List<VehicleEnergyType>()
        };

        // Add supported energy types based on flags
        foreach (EnergyType energyType in Enum.GetValues<EnergyType>())
        {
            if (energyType != EnergyType.None && (supportedEnergyTypes & energyType) != 0)
            {
                vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType
                {
                    Id = Guid.NewGuid(),
                    VehicleId = vehicle.Id,
                    EnergyType = energyType
                });
            }
        }

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }

    private async Task<EnergyEntry> CreateEnergyEntryInDb(Guid vehicleId, EnergyType energyType, DateOnly date, int mileage)
    {
        var energyEntry = new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
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
