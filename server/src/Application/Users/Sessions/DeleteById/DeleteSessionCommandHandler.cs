using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Sessions.DeleteById;

public class DeleteSessionCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<DeleteSessionCommand>
{
    public async Task<Result> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var currentSessionId = userContext.SessionId;

        if (request.SessionId == currentSessionId)
            return Result.Failure(UserErrors.DeleteCurrentSessionFailed);

        var refreshToken = await context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.UserId == userId && rt.Id == request.SessionId, cancellationToken);

        if (refreshToken is null)
            return Result.Failure(UserErrors.SessionNotFound(request.SessionId));

        context.RefreshTokens.Remove(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}