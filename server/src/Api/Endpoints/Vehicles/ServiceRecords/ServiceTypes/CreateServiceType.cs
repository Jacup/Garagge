using Api.Extensions;
using Api.Infrastructure;
using Application.Features.ServiceRecords;
using Application.Features.ServiceTypes.Create;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Vehicles.ServiceRecords.ServiceTypes;

internal sealed class CreateServiceType : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "vehicles/service-records/types",
                async (
                    [FromBody] ServiceTypeCreateRequest request,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new CreateServiceTypeCommand(request.Name);

                    var result = await sender.Send(command, cancellationToken);

                    return result.Match(
                        serviceType => Results.Created($"vehicles/service-records/types/{serviceType.Id}", serviceType),
                        CustomResults.Problem);
                })
            .RequireAuthorization()
            .Produces<ServiceTypeDto>(StatusCodes.Status201Created)
            .WithTags(Tags.ServiceRecords);
    }
}

internal sealed record ServiceTypeCreateRequest(string Name);