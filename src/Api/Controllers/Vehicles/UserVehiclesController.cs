using Api.Endpoints;
using Api.Endpoints.Users;
using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Vehicles;
using Application.Vehicles.CreateMyVehicle;
using Application.Vehicles.GetMyVehicleById;
using Application.Vehicles.GetMyVehicles;
using Infrastructure.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Vehicles;

[ApiController]
[Route("vehicles")]
[Authorize(Permissions.UsersAccess)]
[Tags(Tags.Vehicles)]
public sealed class UserVehiclesController(ISender sender) : ControllerBase
{
    [HttpGet("my")]
    public async Task<IResult> GetMyVehicles(CancellationToken cancellationToken)
    {
        var query = new GetMyVehicles(User.GetUserId());

        Result<ICollection<VehicleDto>> result = await sender.Send(query, cancellationToken);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
    
    [HttpGet("my/{id:guid}")]
    public async Task<IResult> GetMyVehicle(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetMyVehicleByIdQuery(User.GetUserId(), id);

        Result<VehicleDto> result = await sender.Send(query, cancellationToken);

        return result.Match(Results.Ok, CustomResults.Problem);
    }
    
    [HttpPost("my")]
    public async Task<IResult> CreateMyVehicle(CreateMyVehicleCommand command, CancellationToken cancellationToken)
    {
        Result<VehicleDto> result = await sender.Send(command, cancellationToken);

        return result.Match(Results.Created, CustomResults.Problem);
    }
}