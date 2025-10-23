using Domain.Enums;

namespace ApiIntegrationTests.Contracts.V1;

internal sealed record CreateEnergyEntryRequest(
    DateOnly Date,
    int Mileage,
    EnergyType Type,
    EnergyUnit EnergyUnit,
    decimal Volume,
    decimal? Cost,
    decimal? PricePerUnit);
    
internal sealed record UpdateEnergyEntryRequest(
    DateOnly Date,
    int Mileage,
    EnergyType Type,
    EnergyUnit EnergyUnit,
    decimal Volume,
    decimal? Cost,
    decimal? PricePerUnit);