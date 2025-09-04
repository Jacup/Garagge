using Application.Abstractions;
using Domain.Enums;

namespace Application.Services;

public class VehicleEnergyValidator : IVehicleEnergyValidator
{
    public bool CanBeFueled(PowerType powerType) => powerType switch
    {
        PowerType.Gasoline => true,
        PowerType.Diesel => true,
        PowerType.Hybrid => true,
        PowerType.PlugInHybrid => true,
        _ => false
    };

    public bool CanBeCharged(PowerType powerType) => powerType switch
    {
        PowerType.PlugInHybrid => true,
        PowerType.Electric => true,
        _ => false
    };
}