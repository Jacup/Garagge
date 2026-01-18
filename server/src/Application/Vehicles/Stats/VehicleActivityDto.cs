using Application.Dashboard.GetStats;

namespace Application.Vehicles.Stats;

public sealed record VehicleActivityDto
{
    public required ActivityType Type { get; init; }
    public required DateTime Date { get; init; }

    public required ActivityDetail[] ActivityDetails{ get; init; }
}