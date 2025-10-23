using Domain.Enums;

namespace ApiIntegrationTests.Contracts.V1;

internal sealed record CreateVehicleRequest(
    string Brand,
    string Model,
    EngineType EngineType,
    int? ManufacturedYear = null,
    VehicleType? Type = null,
    string? VIN = null,
    IEnumerable<EnergyType>? EnergyTypes = null);
    
internal sealed record UpdateVehicleRequest(
    string Brand,
    string Model,
    EngineType EngineType,
    int? ManufacturedYear = null,
    VehicleType? Type = null,
    string? VIN = null,
    IEnumerable<EnergyType>? EnergyTypes = null);