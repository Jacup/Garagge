using ApiIntegrationTests.Contracts;
using System.Net.Http.Json;

namespace ApiIntegrationTests.Extensions;

internal static class HttpResponseMessageExtensions
{
    internal static async Task<CustomProblemDetails> GetProblemDetailsAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            throw new InvalidOperationException("Successful response");

        var problemDetails = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();

        if (problemDetails == null)
            throw new InvalidOperationException("Response content is not valid ProblemDetails");
        
        return problemDetails;
    }
}