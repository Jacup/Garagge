using Application.Features.EnergyEntries;
using Application.Features.EnergyEntries.GetStats;
using Application.Services.EnergyStats;
using Domain.Entities.EnergyEntries;
using Domain.Enums;
using Domain.Enums.Energy;
using Moq;
using TestUtils.Fakes;

namespace ApplicationTests.Features.EnergyEntries.GetStats;

public class GetEnergyStatsQueryHandlerTests : InMemoryDbTestBase
{
    private readonly GetEnergyStatsQueryHandler _handler;
    private readonly Mock<IEnergyStatsService> _energyStatsServiceMock;

    public GetEnergyStatsQueryHandlerTests()
    {
        _energyStatsServiceMock = new Mock<IEnergyStatsService>();
        TestDateTimeProvider dateTimeProvider = new(new DateTime(2024, 01, 01));

        _handler = new GetEnergyStatsQueryHandler(
            Context,
            UserContextMock.Object,
            _energyStatsServiceMock.Object,
            dateTimeProvider
        );
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsVehicleNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var query = new GetEnergyStatsQuery(Guid.NewGuid(), StatsPeriod.Lifetime);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.VehicleNotFound);
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsVehicleNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline], otherUserId);
        var query = new GetEnergyStatsQuery(vehicle.Id, StatsPeriod.Lifetime);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(EnergyEntryErrors.VehicleNotFound);
    }

    [Fact]
    public async Task Handle_VehicleHasNoEntries_ReturnsEmptyStats()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var query = new GetEnergyStatsQuery(vehicle.Id, StatsPeriod.Lifetime);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.VehicleId.ShouldBe(vehicle.Id);
        result.Value.TotalFuelCost.ShouldBe(0);
        result.Value.TotalEntries.ShouldBe(0);
        result.Value.DistanceDriven.ShouldBe(0);
        result.Value.StatsByType.ShouldBeEmpty();
        result.Value.ChartEntries.ShouldBeEmpty();

        _energyStatsServiceMock.Verify(
            s => s.CalculateTotalCost(It.IsAny<IReadOnlyCollection<EnergyEntry>>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_VehicleHasEntries_DelegatesToEnergyStatsService()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);

        Context.EnergyEntries.Add(new EnergyEntry
        {
            Date = new DateOnly(2023, 12, 29),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id,
            Vehicle = null!,
        });

        await Context.SaveChangesAsync();

        var expectedTotalCost = 200m;
        var expectedDistance = 0;
        var expectedStatsByType = Array.Empty<EnergyTypeStatsDto>();

        _energyStatsServiceMock
            .Setup(s => s.CalculateTotalCost(It.IsAny<IReadOnlyCollection<EnergyEntry>>()))
            .Returns(expectedTotalCost);

        _energyStatsServiceMock
            .Setup(s => s.CalculateDistanceDriven(It.IsAny<IReadOnlyCollection<EnergyEntry>>()))
            .Returns(expectedDistance);

        _energyStatsServiceMock
            .Setup(s => s.CalculateStatsByType(It.IsAny<IReadOnlyCollection<EnergyEntry>>()))
            .Returns(expectedStatsByType);

        var query = new GetEnergyStatsQuery(vehicle.Id, StatsPeriod.Lifetime);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.VehicleId.ShouldBe(vehicle.Id);
        result.Value.TotalFuelCost.ShouldBe(expectedTotalCost);
        result.Value.TotalEntries.ShouldBe(1);
        result.Value.DistanceDriven.ShouldBe(expectedDistance);
        result.Value.StatsByType.ShouldBe(expectedStatsByType);

        _energyStatsServiceMock.Verify(
            s => s.CalculateTotalCost(It.IsAny<IReadOnlyCollection<EnergyEntry>>()),
            Times.Once
        );

        _energyStatsServiceMock.Verify(
            s => s.CalculateDistanceDriven(It.IsAny<IReadOnlyCollection<EnergyEntry>>()),
            Times.Once
        );

        _energyStatsServiceMock.Verify(
            s => s.CalculateStatsByType(It.IsAny<IReadOnlyCollection<EnergyEntry>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_VehicleHasEntries_MapsChartEntriesCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);

        var entry = new EnergyEntry
        {
            Date = new DateOnly(2023, 12, 29),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id,
            Vehicle = null!,
        };

        Context.EnergyEntries.Add(entry);
        await Context.SaveChangesAsync();

        _energyStatsServiceMock
            .Setup(s => s.CalculateTotalCost(It.IsAny<IReadOnlyCollection<EnergyEntry>>()))
            .Returns(200m);

        _energyStatsServiceMock
            .Setup(s => s.CalculateDistanceDriven(It.IsAny<IReadOnlyCollection<EnergyEntry>>()))
            .Returns(0);

        _energyStatsServiceMock
            .Setup(s => s.CalculateStatsByType(It.IsAny<IReadOnlyCollection<EnergyEntry>>()))
            .Returns([]);

        var query = new GetEnergyStatsQuery(vehicle.Id, StatsPeriod.Lifetime);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ChartEntries.Length.ShouldBe(1);

        var chartEntry = result.Value.ChartEntries[0];
        chartEntry.VehicleId.ShouldBe(vehicle.Id);
        chartEntry.Date.ShouldBe(entry.Date);
        chartEntry.Mileage.ShouldBe(entry.Mileage);
        chartEntry.Type.ShouldBe(entry.Type);
        chartEntry.EnergyUnit.ShouldBe(entry.EnergyUnit);
        chartEntry.Volume.ShouldBe(entry.Volume);
        chartEntry.Cost.ShouldBe(entry.Cost);
        chartEntry.PricePerUnit.ShouldBe(entry.PricePerUnit);
        chartEntry.Consumption.ShouldBe(0);
    }

    [Theory]
    [InlineData(StatsPeriod.Week, 1)]
    [InlineData(StatsPeriod.Month, 1)]
    [InlineData(StatsPeriod.Year, 1)]
    [InlineData(StatsPeriod.Lifetime, 2)]
    public async Task Handle_PeriodFilter_FiltersEntriesCorrectly(StatsPeriod period, int expectedEntryCount)
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);

        // 3 days before the mocked "now" (2024-01-01) — within all periods
        var recentEntry = new EnergyEntry
        {
            Date = new DateOnly(2023, 12, 29),
            Mileage = 1000,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 40,
            Cost = 200,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id,
            Vehicle = null!,
        };

        // Over a year before "now" — only included in Lifetime
        var oldEntry = new EnergyEntry
        {
            Date = new DateOnly(2022, 01, 01),
            Mileage = 500,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = 30,
            Cost = 150,
            PricePerUnit = 5m,
            VehicleId = vehicle.Id,
            Vehicle = null!,
        };

        Context.EnergyEntries.AddRange(recentEntry, oldEntry);
        await Context.SaveChangesAsync();

        _energyStatsServiceMock
            .Setup(s => s.CalculateTotalCost(It.IsAny<IReadOnlyCollection<EnergyEntry>>()))
            .Returns(0m);

        _energyStatsServiceMock
            .Setup(s => s.CalculateDistanceDriven(It.IsAny<IReadOnlyCollection<EnergyEntry>>()))
            .Returns(0);

        _energyStatsServiceMock
            .Setup(s => s.CalculateStatsByType(It.IsAny<IReadOnlyCollection<EnergyEntry>>()))
            .Returns([]);

        var query = new GetEnergyStatsQuery(vehicle.Id, period);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalEntries.ShouldBe(expectedEntryCount);
    }
}