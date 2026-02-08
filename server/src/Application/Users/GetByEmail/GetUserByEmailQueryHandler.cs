using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.GetByEmail;

internal sealed class GetUserByEmailQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetUserByEmailQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Email == query.Email && u.Id == userContext.UserId)
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
            return Result.Failure<UserDto>(UserErrors.NotFound);

        return user.Adapt<UserDto>();
    }
}