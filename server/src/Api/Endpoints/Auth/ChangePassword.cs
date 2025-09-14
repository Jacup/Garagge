using Api.Extensions;
using Api.Infrastructure;
using Application.Auth.ChangePassword;
using Application.Core;
using MediatR;

namespace Api.Endpoints.Auth;

internal sealed class ChangePassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("auth/change-password", async (ChangePasswordRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);
                
                Result result = await sender.Send(command, cancellationToken);

                return result.Match(() => Results.Ok(), CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(Tags.Auth);
    }
}

internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);