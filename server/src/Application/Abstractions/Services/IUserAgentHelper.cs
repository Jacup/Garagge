namespace Application.Abstractions.Services;

public interface IUserAgentHelper
{
    public string ParseDeviceName(string? userAgent);
}