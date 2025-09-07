using Domain.Enums;

namespace Application.Abstractions;

public interface IVehicleEnergyValidator
{
    bool CanBeFueled(EngineType engineType);
    bool CanBeCharged(EngineType engineType);
}