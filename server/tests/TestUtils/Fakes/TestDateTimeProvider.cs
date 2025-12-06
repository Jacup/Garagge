using Application.Abstractions;

namespace TestUtils.Fakes;

public class TestDateTimeProvider(DateTime utcNow) : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = utcNow;
}