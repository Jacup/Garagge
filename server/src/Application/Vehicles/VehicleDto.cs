using Domain.Enums;

namespace Application.Vehicles;

public record VehicleDto
{
    public Guid Id { get; init; }
    
    public DateTime CreatedDate { get; init; }
    public DateTime UpdatedDate { get; init; }
    
    public required string Brand { get; init; }
    public required string Model { get; init; }
    public required EngineType EngineType { get; init; }
    
    public int? ManufacturedYear { get; init; }
    public VehicleType? Type { get; init; }
    public string? VIN { get; init; }
    
    public required Guid UserId { get; init; }

    public IEnumerable<EnergyType> AllowedEnergyTypes { get; init; } = [];
}