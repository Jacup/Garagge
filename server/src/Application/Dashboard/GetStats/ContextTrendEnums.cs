using System.Text.Json.Serialization;

namespace Application.Dashboard.GetStats;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContextTrend
{
    Up,
    Down,
    None
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TrendMode
{
    Neutral,
    Good,
    Bad
}