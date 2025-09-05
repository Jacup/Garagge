using Domain.Enums;

namespace Application.Abstractions;

public interface IVehicleEnergyValidator
{
    bool CanBeFueled(EngineType powerType);
    bool CanBeCharged(EngineType powerType);
}