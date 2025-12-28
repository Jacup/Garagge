using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Dashboard.GetStats;
using MediatR;

namespace Api.Endpoints.Miscellaneous;

internal sealed class GetStats : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stats", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetStatsQuery();

                Result<DashboardStatsDto> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces<DashboardStatsDto>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}