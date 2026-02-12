using Api.Extensions;
using Api.Infrastructure;
using Application.Features.ServiceRecords;
using Application.Features.ServiceTypes.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords.ServiceTypes;

internal sealed class UpdateServiceType : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "vehicles/service-records/types/{id:guid}",
                async (
                    [FromRoute] Guid id,
                    [FromBody] ServiceTypeUpdateRequest request,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new UpdateServiceTypeCommand(id,  request.Name);

                    var result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.Ok, CustomResults.Problem);
                })
            .RequireAuthorization()
            .Produces<ServiceTypeDto>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(Tags.ServiceRecords);
    }
}

internal sealed record ServiceTypeUpdateRequest(string Name);