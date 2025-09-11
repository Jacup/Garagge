using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Auth.Login;

internal sealed class LoginUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, ITokenProvider tokenProvider)
    : ICommandHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return Result.Failure<LoginUserResponse>(AuthErrors.WrongEmailOrPassword);
        }
        
        string token = tokenProvider.Create(user);

        return new LoginUserResponse { AccessToken = token };
    }
}