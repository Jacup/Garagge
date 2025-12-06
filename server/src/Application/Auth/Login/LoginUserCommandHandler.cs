using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Core;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Login;

internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IDateTimeProvider dateTimeProvider,
    IUserAgentHelper userAgentHelper)
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
            return Result.Failure<LoginUserResponse>(AuthErrors.WrongEmailOrPassword);

        string token = tokenProvider.Create(user);

        var sessionDuration = command.RememberMe ? LongSessionDurationDays : ShortSessionDurationDays;
        var refreshTokenExpiration = dateTimeProvider.UtcNow.AddDays(sessionDuration);
        
        var deviceName = userAgentHelper.ParseDeviceName(command.UserAgent);
        
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            
            Token = tokenProvider.GenerateRefreshToken(),
            ExpiresAt = refreshTokenExpiration,
            SessionDurationDays = sessionDuration,
            IsRevoked = false,
            
            IpAddress = command.IpAddress,
            UserAgent = command.UserAgent,
            DeviceName = deviceName,
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success(new LoginUserResponse(token, refreshToken.Token, refreshTokenExpiration));
    }
}