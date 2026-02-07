using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Me.Delete;

public class DeleteMeCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<DeleteMeCommand>
{
    public async Task<Result> Handle(DeleteMeCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return Result.Failure(UserErrors.NotFound);

        await context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}