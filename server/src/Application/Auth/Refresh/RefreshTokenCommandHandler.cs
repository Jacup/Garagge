using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Application.Users;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Refresh;

public sealed class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    ITokenProvider tokenProvider,
    IDateTimeProvider dateTimeProvider,
    ILogger<RefreshTokenCommandHandler> logger,
    IUserAgentHelper userAgentHelper)
    : ICommandHandler<RefreshTokenCommand, LoginUserResponse>
{
    public async Task<Result<LoginUserResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var currentToken = await context.RefreshTokens
            .Include(r => r.User)
            .SingleOrDefaultAsync(r => r.Token == command.RefreshToken, cancellationToken);

        if (currentToken is null)
            return Result.Failure<LoginUserResponse>(AuthErrors.InvalidToken);

        if (currentToken.User is null)
        {
            await RevokeAllUserTokens(currentToken.UserId, cancellationToken);

            return Result.Failure<LoginUserResponse>(AuthErrors.UserNotFound);
        }

        if (currentToken.IsRevoked)
        {
            logger.LogWarning("Potential token theft detected for user {UserId}", currentToken.UserId);
            await RevokeAllUserTokens(currentToken.UserId, cancellationToken);

            return Result.Failure<LoginUserResponse>(AuthErrors.TokenRevoked);
        }

        if (currentToken.ExpiresAt <= dateTimeProvider.UtcNow)
        {
            currentToken.IsRevoked = true;
            await context.SaveChangesAsync(cancellationToken);

            return Result.Failure<LoginUserResponse>(AuthErrors.TokenExpired);
        }

        var user = currentToken.User;
        var newAccessToken = tokenProvider.Create(user);
        var newRefreshTokenValue = tokenProvider.GenerateRefreshToken();

        currentToken.IsRevoked = true;
        currentToken.ReplacedByToken = newRefreshTokenValue;

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = dateTimeProvider.UtcNow.AddDays(currentToken.SessionDurationDays),
            SessionDurationDays = currentToken.SessionDurationDays,
            IsRevoked = false,
            IpAddress = command.IpAddress,
            UserAgent = command.UserAgent,
            DeviceName = userAgentHelper.ParseDeviceName(command.UserAgent)
        };

        context.RefreshTokens.Add(newRefreshToken);

        await DeleteExpiredAndRevokedTokens(user.Id, currentToken, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginUserResponse(newAccessToken, newRefreshToken.Token, newRefreshToken.ExpiresAt));
    }

    private async Task RevokeAllUserTokens(Guid userId, CancellationToken cancellationToken)
    {
        var allTokens = await context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in allTokens)
        {
            token.IsRevoked = true;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task DeleteExpiredAndRevokedTokens(Guid userId, RefreshToken currentToken, CancellationToken cancellationToken)
    {
        var tokensToDelete = await context.RefreshTokens
            .Where(rt => rt.UserId == userId &&
                         (rt.ExpiresAt <= dateTimeProvider.UtcNow ||
                          (rt.IsRevoked &&
                           rt.Id != currentToken.Id)))
            .ToListAsync(cancellationToken);

        if (tokensToDelete.Count != 0)
            context.RefreshTokens.RemoveRange(tokensToDelete);
    }
}