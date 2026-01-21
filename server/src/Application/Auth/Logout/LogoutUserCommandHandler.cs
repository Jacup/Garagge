using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Logout;

public class LogoutUserCommandHandler(IApplicationDbContext context) : ICommandHandler<LogoutUserCommand>
{
    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await context.RefreshTokens
            .Where(t => t.Token == request.RefreshToken)
            .ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }
}