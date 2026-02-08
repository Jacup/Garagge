using System.Net.Http.Headers;

namespace ApiIntegrationTests.Extensions;

public static class HttpResponseHeadersExtensions
{
    public static void ShouldContainCookie(this HttpResponseHeaders headers, string cookieName)
    {
        headers.TryGetValues("Set-Cookie", out var cookies).ShouldBeTrue("Response should contain Set-Cookie header");
        
        var cookieList = cookies.ToList();
        cookieList.ShouldNotBeNull();
        cookieList
            .Any(c => c.StartsWith($"{cookieName}="))
            .ShouldBeTrue($"Response should contain cookie '{cookieName}'");
    }
}