using Domain.Enums;

namespace ApiIntegrationTests.Contracts.V1;

internal sealed record VehicleCreateRequest(
    string Brand,
    string Model,
    EngineType EngineType,
    int? ManufacturedYear,
    VehicleType? Type,
    string? VIN,
    IEnumerable<EnergyType>? EnergyTypes);
    
internal sealed record VehicleUpdateRequest(
    string Brand,
    string Model,
    EngineType EngineType,
    int? ManufacturedYear,
    VehicleType? Type,
    string? VIN,
    IEnumerable<EnergyType>? EnergyTypes);