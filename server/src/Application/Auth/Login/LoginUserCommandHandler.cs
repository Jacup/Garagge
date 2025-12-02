using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Login;

internal sealed class LoginUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, ITokenProvider tokenProvider, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return Result.Failure<LoginUserResponse>(AuthErrors.WrongEmailOrPassword);
        }

        string token = tokenProvider.Create(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(), UserId = user.Id, Token = tokenProvider.GenerateRefreshToken(), ExpiresAt = dateTimeProvider.UtcNow.AddDays(7)
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success(new LoginUserResponse(token, refreshToken.Token));
    }
}