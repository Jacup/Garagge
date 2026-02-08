using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Me.Get;

internal sealed class GetMeQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetMeQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userContext.UserId)
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
            return Result.Failure<UserDto>(UserErrors.NotFound);

        return user.Adapt<UserDto>();
    }
}