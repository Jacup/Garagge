using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Update;

public class UpdateUserCommandHandler(IApplicationDbContext context) : ICommandHandler<UpdateUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user == null)
            return Result.Failure<UserDto>(UserErrors.NotFound(request.Id));

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken))
        {
            return Result.Failure<UserDto>(UserErrors.EmailNotUnique);
        }

        user.Email = normalizedEmail;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;


        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<UserDto>(UserErrors.UpdateFailed);
        }

        return user.Adapt<UserDto>();
    }
}