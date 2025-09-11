using Api.Extensions;
using Api.Infrastructure;
using Application.Auth.Register;
using Application.Core;
using MediatR;

namespace Api.Endpoints.Auth;

internal sealed class Register : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", async (RegisterUserCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .Produces<Guid>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict)
        .WithTags(Tags.Auth);
    }
}
