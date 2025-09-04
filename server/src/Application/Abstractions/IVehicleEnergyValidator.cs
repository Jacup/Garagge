using Domain.Enums;

namespace Application.Abstractions;

public interface IVehicleEnergyValidator
{
    bool CanBeFueled(PowerType powerType);
    bool CanBeCharged(PowerType powerType);
}