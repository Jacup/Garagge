using Application.Abstractions.Messaging;

namespace Application.Vehicles.CreateMy;

public sealed record CreateMyVehicleCommand(string Brand, string Model, DateOnly ManufacturedYear) : ICommand<VehicleDto>
{
}