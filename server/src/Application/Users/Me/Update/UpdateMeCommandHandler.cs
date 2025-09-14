using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Me.Update;

public class UpdateMeCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<UpdateMeCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(UpdateMeCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return Result.Failure<UserDto>(UserErrors.NotFound(userId));

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await context.Users.AnyAsync(u => u.Email == normalizedEmail && u.Id != userId, cancellationToken))
        {
            return Result.Failure<UserDto>(UserErrors.EmailNotUnique);
        }

        user.Email = normalizedEmail;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return Result.Failure<UserDto>(UserErrors.UpdateFailed);
        }

        return user.Adapt<UserDto>();
    }
}