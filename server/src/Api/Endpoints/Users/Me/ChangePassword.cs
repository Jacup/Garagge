using Api.Extensions;
using Api.Infrastructure;
using Application.Core;
using Application.Features.Users.Me.ChangePassword;
using MediatR;

namespace Api.Endpoints.Users.Me;

internal sealed class ChangePassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/me/change-password", async (ChangePasswordRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword, request.LogoutAllDevices);

                Result result = await sender.Send(command, cancellationToken);

                return result.Match(() => Results.Ok(), CustomResults.Problem);
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(Tags.Users);
    }
}

internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, bool LogoutAllDevices = false);