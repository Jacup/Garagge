using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Features.Vehicles.Delete;
using MediatR;

namespace Api.Endpoints.Vehicles;

internal sealed class DeleteVehicle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("vehicles/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new DeleteVehicleByIdCommand(id);
                
                Result result = await sender.Send(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags(Tags.Vehicles);
    }
}