using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.Features.ServiceRecords;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ServiceTypes.GetAll;

internal sealed class GetServiceTypesQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetServiceTypesQuery, ICollection<ServiceTypeDto>>
{
    public async Task<Result<ICollection<ServiceTypeDto>>> Handle(GetServiceTypesQuery request, CancellationToken cancellationToken)
    {
        var serviceTypes = await dbContext.ServiceTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return serviceTypes.Adapt<ServiceTypeDto[]>();
    }
}
