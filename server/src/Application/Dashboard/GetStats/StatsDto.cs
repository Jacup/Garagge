using Application.Vehicles.Stats;

namespace Application.Dashboard.GetStats;

public sealed record DashboardStatsDto
{
    public required StatMetricDto FuelExpenses { get; init; }
    public required StatMetricDto DistanceDriven { get; init; }
    public required List<TimelineActivityDto> RecentActivity { get; init; }
}

public sealed record StatMetricDto
{
    public required string Value { get; init; }

    public string? Subtitle { get; init; }

    public string? ContextValue { get; init; }
    public string? ContextAppendText { get; init; }
    public ContextTrend? ContextTrend { get; init; }
    public TrendMode? ContextTrendMode { get; init; }
}

public sealed record TimelineActivityDto
{
    public required Guid VehicleId { get; init; }

    public required ActivityType Type { get; init; }
    public required DateTime Date { get; init; }
    
    public required string Vehicle { get; init; }
    public required ActivityDetail[] ActivityDetails { get; init; }
}