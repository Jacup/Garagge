namespace Application.Dashboard.GetStats;

public sealed record DashboardStatsDto(
    StatMetricDto FuelExpenses,
    StatMetricDto DistanceDriven
    // StatMetricDto MaintenanceAlerts,
    // StatMetricDto Efficiency
);

public sealed record StatMetricDto
{
    public required string Value { get; init; }

    public string? Subtitle { get; init; }

    public string? ContextValue { get; init; }
    public string? ContextAppendText { get; init; }
    public ContextTrend? ContextTrend { get; init; }
    public TrendMode? ContextTrendMode { get; init; }
}