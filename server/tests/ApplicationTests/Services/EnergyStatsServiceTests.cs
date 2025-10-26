using Application.Services.EnergyStats;
using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace ApplicationTests.Services;

public class EnergyStatsServiceTests
{
    private readonly EnergyStatsService _sut = new();

    [Fact]
    public void CalculateAverageConsumption_WithLessThanTwoEntries_ShouldReturnZero()
    {
        // Arrange
        var entries = new List<EnergyEntry> { CreateEnergyEntry(mileage: 1000, volume: 50) };

        // Act
        var result = _sut.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public void CalculateAverageConsumption_WithEmptyList_ShouldReturnZero()
    {
        // Act
        var result = _sut.CalculateAverageConsumption([]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public void CalculateAverageConsumption_WithTwoEntries_ShouldCalculateCorrectly()
    {
        // Arrange
        var entries = new List<EnergyEntry> { CreateEnergyEntry(mileage: 1000, volume: 50), CreateEnergyEntry(mileage: 1500, volume: 30) };

        // Act
        var result = _sut.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(6m);
    }

    [Fact]
    public void CalculateAverageConsumption_WithMultipleEntries_ShouldCalculateAverageCorrectly()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 50), CreateEnergyEntry(mileage: 1500, volume: 30), CreateEnergyEntry(mileage: 2000, volume: 35)
        };

        // Act
        var result = _sut.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(6.5m);
    }

    [Fact]
    public void CalculateAverageConsumption_WithUnsortedEntries_ShouldSortByMileageAndCalculate()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 2000, volume: 35), CreateEnergyEntry(mileage: 1000, volume: 50), CreateEnergyEntry(mileage: 1500, volume: 30)
        };

        // Act
        var result = _sut.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(6.5m);
    }

    [Fact]
    public void CalculateAverageConsumption_WithInvalidDistances_ShouldSkipThoseEntries()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 50), CreateEnergyEntry(mileage: 1000, volume: 30), CreateEnergyEntry(mileage: 1500, volume: 30)
        };

        // Act
        var result = _sut.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(6m);
    }

    [Fact]
    public void CalculateTotalVolume_WithMultipleEntries_ShouldSumVolumes()
    {
        // Arrange
        var entries = new List<EnergyEntry> { CreateEnergyEntry(volume: 50), CreateEnergyEntry(volume: 30), CreateEnergyEntry(volume: 40) };

        // Act
        var result = _sut.CalculateTotalVolume(entries);

        // Assert
        result.ShouldBe(120m);
    }

    [Fact]
    public void CalculateTotalVolume_WithEmptyList_ShouldReturnZero()
    {
        // Act
        var result = _sut.CalculateTotalVolume([]);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateTotalCost_WithMultipleEntries_ShouldSumCosts()
    {
        // Arrange
        var entries = new List<EnergyEntry> { CreateEnergyEntry(cost: 100), CreateEnergyEntry(cost: 150), CreateEnergyEntry(cost: 200) };

        // Act
        var result = _sut.CalculateTotalCost(entries);

        // Assert
        result.ShouldBe(450m);
    }

    [Fact]
    public void CalculateTotalCost_WithNullCosts_ShouldIgnoreNulls()
    {
        // Arrange
        var entries = new List<EnergyEntry> { CreateEnergyEntry(cost: 100), CreateEnergyEntry(cost: null), CreateEnergyEntry(cost: 200) };

        // Act
        var result = _sut.CalculateTotalCost(entries);

        // Assert
        result.ShouldBe(300m);
    }

    [Fact]
    public void CalculateTotalCost_WithEmptyList_ShouldReturnZero()
    {
        // Act
        var result = _sut.CalculateTotalCost([]);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateAveragePricePerUnit_WithMultipleEntries_ShouldCalculateAverage()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(pricePerUnit: 5.5m), CreateEnergyEntry(pricePerUnit: 6.0m), CreateEnergyEntry(pricePerUnit: 5.0m)
        };

        // Act
        var result = _sut.CalculateAveragePricePerUnit(entries);

        // Assert
        result.ShouldBe(5.5m);
    }

    [Fact]
    public void CalculateAveragePricePerUnit_WithNullPrices_ShouldIgnoreNulls()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(pricePerUnit: 5.0m), CreateEnergyEntry(pricePerUnit: null), CreateEnergyEntry(pricePerUnit: 7.0m)
        };

        // Act
        var result = _sut.CalculateAveragePricePerUnit(entries);

        // Assert
        result.ShouldBe(6.0m);
    }

    [Fact]
    public void CalculateAveragePricePerUnit_WithAllNullPrices_ShouldReturnZero()
    {
        // Arrange
        var entries = new List<EnergyEntry> { CreateEnergyEntry(pricePerUnit: null), CreateEnergyEntry(pricePerUnit: null) };

        // Act
        var result = _sut.CalculateAveragePricePerUnit(entries);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateAveragePricePerUnit_WithEmptyList_ShouldReturnZero()
    {
        // Act
        var result = _sut.CalculateAveragePricePerUnit([]);

        // Assert
        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithNullEntries_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, null!));
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithEmptyList_ShouldReturnZeroStats()
    {
        // Arrange
        var entries = new List<EnergyEntry>();

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, entries);

        // Assert
        result.Unit.ShouldBe(EnergyUnit.Liter);
        result.EntriesCount.ShouldBe(0);
        result.EnergyTypes.ShouldBeEmpty();
        result.TotalVolume.ShouldBe(0);
        result.TotalCost.ShouldBe(0);
        result.AverageConsumption.ShouldBe(0);
        result.AveragePricePerUnit.ShouldBe(0);
        result.AverageCostPer100km.ShouldBe(0);
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithSingleEntry_ShouldReturnZeroConsumption()
    {
        // Arrange
        var entries = new List<EnergyEntry> { CreateEnergyEntry(mileage: 1000, volume: 50, cost: 250, pricePerUnit: 5m) };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, entries);

        // Assert
        result.Unit.ShouldBe(EnergyUnit.Liter);
        result.EntriesCount.ShouldBe(1);
        result.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        result.TotalVolume.ShouldBe(50);
        result.TotalCost.ShouldBe(250);
        result.AverageConsumption.ShouldBe(0); // Can't calculate with single entry
        result.AveragePricePerUnit.ShouldBe(5m);
        result.AverageCostPer100km.ShouldBe(0); // No consumption = no cost per 100km
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithTwoEntries_ShouldCalculateAllMetrics()
    {
        // Arrange
        // Mileage: 1000 -> 1500 (500 km)
        // Volume: 50 -> 60 liters
        // Consumption: (60/500)*100 = 12 l/100km
        // Price: 5 PLN/liter
        // Cost per 100km: (12/100)*5 = 0.6 PLN/km
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 50, cost: 250, pricePerUnit: 5m),
            CreateEnergyEntry(mileage: 1500, volume: 60, cost: 300, pricePerUnit: 5m)
        };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, entries);

        // Assert
        result.Unit.ShouldBe(EnergyUnit.Liter);
        result.EntriesCount.ShouldBe(2);
        result.EnergyTypes.Count.ShouldBe(1);
        result.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        result.TotalVolume.ShouldBe(110);
        result.TotalCost.ShouldBe(550);
        result.AverageConsumption.ShouldBe(12m);
        result.AveragePricePerUnit.ShouldBe(5m);
        result.AverageCostPer100km.ShouldBe(0.6m);
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithMultipleEntries_ShouldCalculateAverages()
    {
        // Arrange
        // Entry 1: 1000 km, 50 l, 250 PLN, 5 PLN/l
        // Entry 2: 1500 km, 30 l, 150 PLN, 5 PLN/l -> consumption: (30/500)*100 = 6 l/100km
        // Entry 3: 2000 km, 35 l, 175 PLN, 5 PLN/l -> consumption: (35/500)*100 = 7 l/100km
        // Average consumption: (6+7)/2 = 6.5 l/100km
        // Average price: 5 PLN/l
        // Cost per 100km: (6.5/100)*5 = 0.325 PLN/km
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 50, cost: 250, pricePerUnit: 5m),
            CreateEnergyEntry(mileage: 1500, volume: 30, cost: 150, pricePerUnit: 5m),
            CreateEnergyEntry(mileage: 2000, volume: 35, cost: 175, pricePerUnit: 5m)
        };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, entries);

        // Assert
        result.Unit.ShouldBe(EnergyUnit.Liter);
        result.EntriesCount.ShouldBe(3);
        result.TotalVolume.ShouldBe(115);
        result.TotalCost.ShouldBe(575);
        result.AverageConsumption.ShouldBe(6.5m);
        result.AveragePricePerUnit.ShouldBe(5m);
        result.AverageCostPer100km.ShouldBe(0.325m);
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithMultipleEnergyTypes_ShouldIncludeAllTypes()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 40, energyType: EnergyType.Gasoline),
            CreateEnergyEntry(mileage: 1500, volume: 60, energyType: EnergyType.Diesel),
            CreateEnergyEntry(mileage: 2000, volume: 50, energyType: EnergyType.Gasoline)
        };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, entries);

        // Assert
        result.Unit.ShouldBe(EnergyUnit.Liter);
        result.EntriesCount.ShouldBe(3);
        result.EnergyTypes.Count.ShouldBe(2);
        result.EnergyTypes.ShouldContain(EnergyType.Gasoline);
        result.EnergyTypes.ShouldContain(EnergyType.Diesel);
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithNullCosts_ShouldIgnoreNulls()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 40, cost: null, pricePerUnit: null),
            CreateEnergyEntry(mileage: 1500, volume: 60, cost: 300, pricePerUnit: 5m)
        };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, entries);

        // Assert
        result.TotalVolume.ShouldBe(100);
        result.TotalCost.ShouldBe(300); // Only counts non-null costs
        result.AveragePricePerUnit.ShouldBe(5m); // Only counts non-null prices
        result.AverageConsumption.ShouldBe(12m); // (60/500)*100
        result.AverageCostPer100km.ShouldBe(0.6m); // (12/100)*5
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithZeroConsumption_ShouldReturnZeroCostPer100km()
    {
        // Arrange - single entry means zero consumption
        var entries = new List<EnergyEntry> { CreateEnergyEntry(mileage: 1000, volume: 50, cost: 250, pricePerUnit: 5m) };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, entries);

        // Assert
        result.AverageConsumption.ShouldBe(0);
        result.AveragePricePerUnit.ShouldBe(5m);
        result.AverageCostPer100km.ShouldBe(0); // No consumption = no cost per 100km
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithZeroPrice_ShouldReturnZeroCostPer100km()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 50, cost: 0, pricePerUnit: 0), CreateEnergyEntry(mileage: 1500, volume: 60, cost: 0, pricePerUnit: 0)
        };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, entries);

        // Assert
        result.AverageConsumption.ShouldBe(12m); // (60/500)*100
        result.AveragePricePerUnit.ShouldBe(0);
        result.AverageCostPer100km.ShouldBe(0); // Price is 0, so cost per 100km is 0
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithDifferentUnit_ShouldUseSpecifiedUnit()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 10, energyUnit: EnergyUnit.Gallon),
            CreateEnergyEntry(mileage: 1500, volume: 15, energyUnit: EnergyUnit.Gallon)
        };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Gallon, entries);

        // Assert
        result.Unit.ShouldBe(EnergyUnit.Gallon);
        result.TotalVolume.ShouldBe(25);
        result.AverageConsumption.ShouldBe(3m); // (15/500)*100 = 3 gallons/100km
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithElectricEnergy_ShouldCalculateKWhMetrics()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 50, cost: 100, pricePerUnit: 2m,
                energyType: EnergyType.Electric, energyUnit: EnergyUnit.kWh),
            CreateEnergyEntry(mileage: 1500, volume: 60, cost: 120, pricePerUnit: 2m,
                energyType: EnergyType.Electric, energyUnit: EnergyUnit.kWh)
        };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.kWh, entries);

        // Assert
        result.Unit.ShouldBe(EnergyUnit.kWh);
        result.EnergyTypes.ShouldContain(EnergyType.Electric);
        result.TotalVolume.ShouldBe(110);
        result.TotalCost.ShouldBe(220);
        result.AverageConsumption.ShouldBe(12m); // (60/500)*100 = 12 kWh/100km
        result.AveragePricePerUnit.ShouldBe(2m);
        result.AverageCostPer100km.ShouldBe(0.24m); // (12/100)*2 = 0.24
    }

    [Fact]
    public void CalculateStatisticsForUnit_EnergyTypesShouldBeOrdered()
    {
        // Arrange - Add types in random order
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 40, energyType: EnergyType.Electric), // 8
            CreateEnergyEntry(mileage: 1500, volume: 60, energyType: EnergyType.Gasoline), // 1
            CreateEnergyEntry(mileage: 2000, volume: 50, energyType: EnergyType.Diesel) // 2
        };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, entries);

        // Assert
        result.EnergyTypes.Count.ShouldBe(3);
        // Should be ordered by enum value
        result.EnergyTypes.ElementAt(0).ShouldBe(EnergyType.Gasoline); // 1
        result.EnergyTypes.ElementAt(1).ShouldBe(EnergyType.Diesel); // 2
        result.EnergyTypes.ElementAt(2).ShouldBe(EnergyType.Electric); // 8
    }

    [Fact]
    public void CalculateStatisticsForUnit_WithVaryingPrices_ShouldCalculateCorrectAverages()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 50, cost: 250, pricePerUnit: 5.0m),
            CreateEnergyEntry(mileage: 1500, volume: 60, cost: 360, pricePerUnit: 6.0m),
            CreateEnergyEntry(mileage: 2000, volume: 50, cost: 200, pricePerUnit: 4.0m)
        };

        // Act
        var result = _sut.CalculateStatisticsForUnit(EnergyUnit.Liter, entries);

        // Assert
        result.TotalVolume.ShouldBe(160);
        result.TotalCost.ShouldBe(810);
        result.AveragePricePerUnit.ShouldBe(5.0m); // (5+6+4)/3 = 5
        // Consumption 1: (60/500)*100 = 12 l/100km
        // Consumption 2: (50/500)*100 = 10 l/100km
        // Average: (12+10)/2 = 11 l/100km
        result.AverageConsumption.ShouldBe(11m);
        result.AverageCostPer100km.ShouldBe(0.55m); // (11/100)*5 = 0.55
    }

    private static EnergyEntry CreateEnergyEntry(
        int mileage = 1000,
        decimal volume = 50,
        decimal? cost = 100,
        decimal? pricePerUnit = 2.0m,
        EnergyType energyType = EnergyType.Gasoline,
        EnergyUnit energyUnit = EnergyUnit.Liter)
    {
        return new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2024, 1, 1),
            Mileage = mileage,
            Type = energyType,
            EnergyUnit = energyUnit,
            Volume = volume,
            Cost = cost,
            PricePerUnit = pricePerUnit
        };
    }
}