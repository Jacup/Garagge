using Api.Extensions;
using Api.Infrastructure;
using Application.Features.ServiceTypes.Delete;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords.ServiceTypes;

internal sealed class DeleteServiceType : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "vehicles/service-records/types/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new DeleteServiceTypeCommand(id);

                    var result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.NoContent, CustomResults.Problem);
                })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tags.ServiceRecords);
    }
}