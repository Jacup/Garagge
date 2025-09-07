using Domain.Enums;

namespace Application.VehicleEnergyTypes;

public sealed record VehicleEnergyTypeDto
{
    public required Guid VehicleId { get; init; }

    public required EnergyType EnergyType { get; init; }
}