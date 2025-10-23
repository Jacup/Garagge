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
        app.MapPost("auth/register", async (RegisterRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new RegisterUserCommand(request.Email, request.FirstName, request.LastName, request.Password);

                Result<Guid> result = await sender.Send(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .AllowAnonymous()
            .Produces<Guid>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .WithTags(Tags.Auth);
    }
}

internal sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName);
