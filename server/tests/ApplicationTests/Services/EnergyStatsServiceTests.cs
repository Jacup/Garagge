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
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 50)
        };

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
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(mileage: 1000, volume: 50),
            CreateEnergyEntry(mileage: 1500, volume: 30)
        };

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
            CreateEnergyEntry(mileage: 1000, volume: 50),
            CreateEnergyEntry(mileage: 1500, volume: 30),
            CreateEnergyEntry(mileage: 2000, volume: 35)
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
            CreateEnergyEntry(mileage: 2000, volume: 35),
            CreateEnergyEntry(mileage: 1000, volume: 50),
            CreateEnergyEntry(mileage: 1500, volume: 30)
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
            CreateEnergyEntry(mileage: 1000, volume: 50),
            CreateEnergyEntry(mileage: 1000, volume: 30),
            CreateEnergyEntry(mileage: 1500, volume: 30) 
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
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(volume: 50),
            CreateEnergyEntry(volume: 30),
            CreateEnergyEntry(volume: 40)
        };

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
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(cost: 100),
            CreateEnergyEntry(cost: 150),
            CreateEnergyEntry(cost: 200)
        };

        // Act
        var result = _sut.CalculateTotalCost(entries);

        // Assert
        result.ShouldBe(450m);
    }

    [Fact]
    public void CalculateTotalCost_WithNullCosts_ShouldIgnoreNulls()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(cost: 100),
            CreateEnergyEntry(cost: null),
            CreateEnergyEntry(cost: 200)
        };

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
            CreateEnergyEntry(pricePerUnit: 5.5m),
            CreateEnergyEntry(pricePerUnit: 6.0m),
            CreateEnergyEntry(pricePerUnit: 5.0m)
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
            CreateEnergyEntry(pricePerUnit: 5.0m),
            CreateEnergyEntry(pricePerUnit: null),
            CreateEnergyEntry(pricePerUnit: 7.0m)
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
        var entries = new List<EnergyEntry>
        {
            CreateEnergyEntry(pricePerUnit: null),
            CreateEnergyEntry(pricePerUnit: null)
        };

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

    private static EnergyEntry CreateEnergyEntry(
        int mileage = 1000,
        decimal volume = 50,
        decimal? cost = 100,
        decimal? pricePerUnit = 2.0m)
    {
        return new EnergyEntry
        {
            Id = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2024, 1, 1),
            Mileage = mileage,
            Type = EnergyType.Gasoline,
            EnergyUnit = EnergyUnit.Liter,
            Volume = volume,
            Cost = cost,
            PricePerUnit = pricePerUnit
        };
    }
}

