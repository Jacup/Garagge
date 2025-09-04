using Application.Services;
using Domain.Enums;

namespace ApplicationTests.Services;

public class VehicleEnergyValidatorTests
{
    private readonly VehicleEnergyValidator _validator = new();

    [Theory]
    [InlineData(PowerType.Gasoline, true)]
    [InlineData(PowerType.Diesel, true)]
    [InlineData(PowerType.Hybrid, true)]
    [InlineData(PowerType.PlugInHybrid, true)]
    [InlineData(PowerType.Electric, false)]
    public void CanBeFueled_ShouldReturnExpected(PowerType powerType, bool expected)
    {
        _validator.CanBeFueled(powerType).ShouldBe(expected);
    }

    [Theory]
    [InlineData(PowerType.PlugInHybrid, true)]
    [InlineData(PowerType.Electric, true)]
    [InlineData(PowerType.Gasoline, false)]
    [InlineData(PowerType.Diesel, false)]
    [InlineData(PowerType.Hybrid, false)]
    public void CanBeCharged_ShouldReturnExpected(PowerType powerType, bool expected)
    {
        _validator.CanBeCharged(powerType).ShouldBe(expected);
    }
}