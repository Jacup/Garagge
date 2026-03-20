using Application.Services.EnergyStats;
using Domain.Entities.EnergyEntries;
using Domain.Enums;

namespace ApplicationTests.Services;

public class EnergyStatsServiceTests
{
    private readonly EnergyStatsService _sut = new();

    #region CalculateTotalCost

    [Fact]
    public void CalculateTotalCost_EmptyList_ReturnsZero()
    {
        var result = _sut.CalculateTotalCost([]);

        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateTotalCost_MultipleEntries_SumsAllCosts()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(cost: 100), CreateEntry(cost: 150), CreateEntry(cost: 200)
        };

        var result = _sut.CalculateTotalCost(entries);

        result.ShouldBe(450m);
    }

    [Fact]
    public void CalculateTotalCost_EntriesWithNullCost_IgnoresNulls()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(cost: 100), CreateEntry(cost: null), CreateEntry(cost: 200)
        };

        var result = _sut.CalculateTotalCost(entries);

        result.ShouldBe(300m);
    }

    #endregion

    #region CalculateDistanceDriven

    [Fact]
    public void CalculateDistanceDriven_EmptyList_ReturnsZero()
    {
        var result = _sut.CalculateDistanceDriven([]);

        result.ShouldBe(0);
    }

    [Fact]
    public void CalculateDistanceDriven_SingleEntry_ReturnsZero()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000)
        };

        var result = _sut.CalculateDistanceDriven(entries);

        result.ShouldBe(0);
    }

    [Fact]
    public void CalculateDistanceDriven_MultipleEntries_ReturnsMaxMinusMínMileage()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000), CreateEntry(mileage: 1500), CreateEntry(mileage: 2000)
        };

        var result = _sut.CalculateDistanceDriven(entries);

        result.ShouldBe(1000);
    }

    [Fact]
    public void CalculateDistanceDriven_UnsortedEntries_ReturnsCorrectDistance()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 2000), CreateEntry(mileage: 1000), CreateEntry(mileage: 1500)
        };

        var result = _sut.CalculateDistanceDriven(entries);

        result.ShouldBe(1000);
    }

    #endregion

    #region CalculateStatsByType

    [Fact]
    public void CalculateStatsByType_EmptyList_ReturnsEmptyArray()
    {
        var result = _sut.CalculateStatsByType([]);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void CalculateStatsByType_SingleType_ReturnsSingleGroupWithCorrectStats()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000, volume: 40, cost: 200, pricePerUnit: 5m), CreateEntry(mileage: 1500, volume: 60, cost: 300, pricePerUnit: 5m)
        };

        var result = _sut.CalculateStatsByType(entries);

        result.Length.ShouldBe(1);
        var stat = result[0];
        stat.Type.ShouldBe(EnergyType.Gasoline);
        stat.ItemsCount.ShouldBe(2);
        stat.TotalVolume.ShouldBe(100);
        stat.TotalCost.ShouldBe(500);
        stat.AverageConsumption.ShouldBe(12m); // (60/500)*100
        stat.AveragePricePerUnit.ShouldBe(5m);
        stat.AverageCostPer100km.ShouldBe(0.6m); // (12/100)*5
    }

    [Fact]
    public void CalculateStatsByType_MultipleTypes_ReturnsOneGroupPerType()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000, volume: 40, cost: 200, energyType: EnergyType.Gasoline),
            CreateEntry(mileage: 1500, volume: 60, cost: 240, energyType: EnergyType.Diesel),
            CreateEntry(mileage: 2000, volume: 50, cost: 200, energyType: EnergyType.Gasoline)
        };

        var result = _sut.CalculateStatsByType(entries);

        result.Length.ShouldBe(2);
        result.ShouldContain(s => s.Type == EnergyType.Gasoline);
        result.ShouldContain(s => s.Type == EnergyType.Diesel);
    }

    [Fact]
    public void CalculateStatsByType_MultipleTypes_EachGroupHasCorrectItemCount()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000, volume: 40, energyType: EnergyType.Gasoline),
            CreateEntry(mileage: 1500, volume: 60, energyType: EnergyType.Diesel),
            CreateEntry(mileage: 2000, volume: 50, energyType: EnergyType.Gasoline)
        };

        var result = _sut.CalculateStatsByType(entries);

        result.First(s => s.Type == EnergyType.Gasoline).ItemsCount.ShouldBe(2);
        result.First(s => s.Type == EnergyType.Diesel).ItemsCount.ShouldBe(1);
    }

    [Fact]
    public void CalculateStatsByType_EntriesWithNullCosts_IgnoresNullsInCostCalculation()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000, volume: 40, cost: null, pricePerUnit: null), CreateEntry(mileage: 1500, volume: 60, cost: 300, pricePerUnit: 5m)
        };

        var result = _sut.CalculateStatsByType(entries);

        var stat = result[0];
        stat.TotalCost.ShouldBe(300);
        stat.AveragePricePerUnit.ShouldBe(5m);
    }

    [Fact]
    public void CalculateStatsByType_ZeroConsumptionOrPrice_ReturnZeroCostPer100km()
    {
        // Single entry — consumption will be 0
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000, volume: 50, cost: 250, pricePerUnit: 5m)
        };

        var result = _sut.CalculateStatsByType(entries);

        result[0].AverageCostPer100km.ShouldBe(0);
    }

    #endregion

    #region CalculateAverageConsumption

    [Fact]
    public void CalculateAverageConsumption_EmptyList_ReturnsZero()
    {
        var result = _sut.CalculateAverageConsumption([]);

        result.ShouldBe(0);
    }

    [Fact]
    public void CalculateAverageConsumption_LessThanTwoEntries_ReturnsZero()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000, volume: 50)
        };

        var result = _sut.CalculateAverageConsumption(entries);

        result.ShouldBe(0);
    }

    [Fact]
    public void CalculateAverageConsumption_TwoEntries_ReturnsCorrectValue()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000, volume: 50), CreateEntry(mileage: 1500, volume: 30)
        };

        var result = _sut.CalculateAverageConsumption(entries);

        result.ShouldBe(6m); // (30/500)*100
    }

    [Fact]
    public void CalculateAverageConsumption_MultipleEntries_ReturnsAverageOfSegments()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000, volume: 50), CreateEntry(mileage: 1500, volume: 30), CreateEntry(mileage: 2000, volume: 35)
        };

        var result = _sut.CalculateAverageConsumption(entries);

        // Segment 1: (30/500)*100 = 6, Segment 2: (35/500)*100 = 7, avg = 6.5
        result.ShouldBe(6.5m);
    }

    [Fact]
    public void CalculateAverageConsumption_UnsortedEntries_SortsByMileageBeforeCalculating()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 2000, volume: 35), CreateEntry(mileage: 1000, volume: 50), CreateEntry(mileage: 1500, volume: 30)
        };

        var result = _sut.CalculateAverageConsumption(entries);

        result.ShouldBe(6.5m);
    }

    [Fact]
    public void CalculateAverageConsumption_EntriesWithSameMileage_SkipsZeroDistanceSegments()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(mileage: 1000, volume: 50),
            CreateEntry(mileage: 1000, volume: 30), // same mileage — should be skipped
            CreateEntry(mileage: 1500, volume: 30)
        };

        var result = _sut.CalculateAverageConsumption(entries);

        result.ShouldBe(6m); // only (30/500)*100
    }

    #endregion

    #region CalculateAveragePricePerUnit

    [Fact]
    public void CalculateAveragePricePerUnit_EmptyList_ReturnsZero()
    {
        var result = _sut.CalculateAveragePricePerUnit([]);

        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateAveragePricePerUnit_AllNullPrices_ReturnsZero()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(pricePerUnit: null), CreateEntry(pricePerUnit: null)
        };

        var result = _sut.CalculateAveragePricePerUnit(entries);

        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateAveragePricePerUnit_MultipleEntries_ReturnsAverage()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(pricePerUnit: 5.5m), CreateEntry(pricePerUnit: 6.0m), CreateEntry(pricePerUnit: 5.0m)
        };

        var result = _sut.CalculateAveragePricePerUnit(entries);

        result.ShouldBe(5.5m);
    }

    [Fact]
    public void CalculateAveragePricePerUnit_EntriesWithNullPrices_IgnoresNulls()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(pricePerUnit: 5.0m), CreateEntry(pricePerUnit: null), CreateEntry(pricePerUnit: 7.0m)
        };

        var result = _sut.CalculateAveragePricePerUnit(entries);

        result.ShouldBe(6.0m);
    }

    #endregion

    #region CalculateTotalVolume

    [Fact]
    public void CalculateTotalVolume_EmptyList_ReturnsZero()
    {
        var result = _sut.CalculateTotalVolume([]);

        result.ShouldBe(0m);
    }

    [Fact]
    public void CalculateTotalVolume_MultipleEntries_SumsAllVolumes()
    {
        var entries = new List<EnergyEntry>
        {
            CreateEntry(volume: 50), CreateEntry(volume: 30), CreateEntry(volume: 40)
        };

        var result = _sut.CalculateTotalVolume(entries);

        result.ShouldBe(120m);
    }

    #endregion

    private static EnergyEntry CreateEntry(
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
            Vehicle = null!,
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