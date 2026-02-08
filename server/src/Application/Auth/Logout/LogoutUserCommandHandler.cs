using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Logout;

public class LogoutUserCommandHandler(IApplicationDbContext context) : ICommandHandler<LogoutUserCommand>
{
    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (token is null)
            return Result.Success();

        token.IsRevoked = true;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}