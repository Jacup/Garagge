using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using UAParser;

namespace Application.Auth.Login;

internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<LoginUserCommand, LoginUserResponse>
{
    private const int ShortSessionDurationDays = 1;
    private const int LongSessionDurationDays = 30;

    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
            return Result.Failure<LoginUserResponse>(AuthErrors.CredentialsInvalid);

        var sessionId = Guid.NewGuid();
        var sessionDuration = command.RememberMe ? LongSessionDurationDays : ShortSessionDurationDays;
        var refreshTokenExpiration = dateTimeProvider.UtcNow.AddDays(sessionDuration);

        var uaParser = Parser.GetDefault();
        var clientInfo = uaParser.Parse(command.UserAgent ?? "");

        var refreshToken = new RefreshToken
        {
            Id = sessionId,
            UserId = user.Id,
            Token = tokenProvider.GenerateRefreshToken(),
            ExpiresAt = refreshTokenExpiration,
            SessionDurationDays = sessionDuration,
            IsRevoked = false,
            UserAgent = command.UserAgent,
            DeviceOs = clientInfo?.OS.ToString(),
            DeviceBrowser = clientInfo?.UA.Family,
            DeviceType = clientInfo?.Device.ToString(),
            IpAddress = command.IpAddress
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        string token = tokenProvider.Create(user, sessionId);

        return new LoginUserResponse(token, refreshToken.Token, refreshTokenExpiration);
    }
}