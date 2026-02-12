using ApiIntegrationTests.Contracts;
using Application.Core;
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

    public static void ShouldContainError(this CustomProblemDetails problemDetails, Error expectedError)
    {
        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.ShouldContain(
            error => error.Code == expectedError.Code,
            $"Expected error with code '{expectedError.Code}' but it was not found. " +
            $"Actual errors: {string.Join(", ", problemDetails.Errors?.Select(e => e.Code) ?? [])}");
    }

    public static void ShouldContainOnlyError(this CustomProblemDetails problemDetails, Error expectedError)
    {
        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(1);
        problemDetails.Errors[0].Code.ShouldBe(expectedError.Code);
        problemDetails.Errors[0].Description.ShouldBe(expectedError.Description);
    }

    public static void ShouldContainErrors(this CustomProblemDetails problemDetails, params Error[] expectedErrors)
    {
        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(expectedErrors.Length);

        foreach (var expectedError in expectedErrors)
        {
            problemDetails.Errors.ShouldContain(
                e => e.Code == expectedError.Code,
                $"Expected error with code '{expectedError.Code}' but it was not found.");
        }
    }
}