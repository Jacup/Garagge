using Application.Abstractions.Messaging;

namespace Application.Vehicles.CreateMyVehicle;

public sealed record CreateMyVehicleCommand(string Brand, string Model, int? ManufacturedYear) : ICommand<VehicleDto>
{
}