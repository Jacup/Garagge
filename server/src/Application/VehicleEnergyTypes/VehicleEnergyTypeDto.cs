using Domain.Enums;

namespace Application.VehicleEnergyTypes;

public sealed record VehicleEnergyTypeDto
{
    public required Guid Id { get; init; }
    public required Guid VehicleId { get; init; }
    
    public DateTime CreatedDate { get; init; }
    public DateTime UpdatedDate { get; init; }

    public required EnergyType EnergyType { get; init; }
}