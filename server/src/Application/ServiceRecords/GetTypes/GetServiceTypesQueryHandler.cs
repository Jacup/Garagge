using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.ServiceRecords.GetTypes;

public sealed class GetServiceTypesQueryHandler(IApplicationDbContext dbContext, ILogger<GetServiceTypesQueryHandler> logger)
    : IQueryHandler<GetServiceTypesQuery, ICollection<ServiceTypeDto>>
{
    public async Task<Result<ICollection<ServiceTypeDto>>> Handle(GetServiceTypesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var serviceTypes = await dbContext.ServiceTypes
                .AsNoTracking()
                .ProjectToType<ServiceTypeDto>()
                .ToListAsync(cancellationToken);

            return Result.Success<ICollection<ServiceTypeDto>>(serviceTypes);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception occurred while retrieving service types.");
            return Result.Failure<ICollection<ServiceTypeDto>>(ServiceRecordErrors.GetTypesFailed);
        }
    }
}