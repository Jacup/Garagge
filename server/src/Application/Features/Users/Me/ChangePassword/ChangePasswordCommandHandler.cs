using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Me.ChangePassword;

internal sealed class ChangePasswordCommandHandler(IApplicationDbContext context, IUserContext userContext, IPasswordHasher passwordHasher)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (user == null)
            return Result.Failure(UserErrors.NotFound);

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Failure(UserErrors.PasswordInvalid);

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);

        await context.SaveChangesAsync(cancellationToken);

        if (request.LogoutAllDevices)
        {
            await context.RefreshTokens
                .Where(rt => rt.UserId == user.Id &&
                             rt.Id != userContext.SessionId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}