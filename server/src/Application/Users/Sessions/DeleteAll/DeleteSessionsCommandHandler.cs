using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Sessions.DeleteAll;

internal class DeleteSessionsCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<DeleteSessionsCommand>
{
    public async Task<Result> Handle(DeleteSessionsCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var currentSessionId = userContext.SessionId;

        await context.RefreshTokens
            .Where(rt => rt.UserId == userId &&
                         rt.Id != currentSessionId)
            .ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }
}