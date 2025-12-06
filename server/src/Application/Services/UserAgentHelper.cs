using Application.Abstractions.Services;

namespace Application.Services;

public class UserAgentHelper : IUserAgentHelper
{
    public string ParseDeviceName(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) 
            return "Unknown Device";

        var ua = userAgent.ToLower();
        string os = "Unknown OS";
        string browser = "Unknown Browser";

        if (ua.Contains("windows")) 
            os = "Windows";
        else if (ua.Contains("mac os") && !ua.Contains("iphone") && !ua.Contains("ipad")) 
            os = "macOS";
        else if (ua.Contains("android")) 
            os = "Android";
        else if (ua.Contains("iphone") || ua.Contains("ipad")) 
            os = "iOS";
        else if (ua.Contains("linux")) 
            os = "Linux";

        if (ua.Contains("edg/")) 
            browser = "Edge";
        else if (ua.Contains("chrome") && !ua.Contains("edg/") && !ua.Contains("opr/")) 
            browser = "Chrome";
        else if (ua.Contains("firefox")) 
            browser = "Firefox";
        else if (ua.Contains("safari") && !ua.Contains("chrome")) 
            browser = "Safari";
        else if (ua.Contains("opr/") || ua.Contains("opera")) 
            browser = "Opera";

        return $"{browser} on {os}";
    }
}