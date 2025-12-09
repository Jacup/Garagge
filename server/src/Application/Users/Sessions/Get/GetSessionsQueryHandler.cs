using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Sessions.Get;

public class GetSessionsQueryHandler(IApplicationDbContext context, IUserContext userContext) : IQueryHandler<GetSessionsQuery, SessionsDto>
{
    public async Task<Result<SessionsDto>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var currentSessionId = userContext.SessionId;

        var sessions = await context.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .OrderByDescending(rt => rt.CreatedDate)
            .Select(rt => new SessionDto
            {
                Id = rt.Id,
                CreatedDate = rt.CreatedDate,
                IsCurrent = rt.Id == currentSessionId,
                DeviceOs = rt.DeviceOs,
                DeviceBrowser = rt.DeviceBrowser,
                DeviceType = rt.DeviceType,
                IpAddress = rt.IpAddress,
                Location = rt.Location
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new SessionsDto(sessions));
    }
}