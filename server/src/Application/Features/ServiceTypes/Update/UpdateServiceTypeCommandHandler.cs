using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.Features.ServiceRecords;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ServiceTypes.Update;

internal sealed class UpdateServiceTypeCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<UpdateServiceTypeCommand, ServiceTypeDto>
{
    public async Task<Result<ServiceTypeDto>> Handle(UpdateServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ServiceTypes.FirstOrDefaultAsync(st => st.Id == request.Id, cancellationToken: cancellationToken);

        if (entity == null)
            return Result.Failure<ServiceTypeDto>(ServiceTypeErrors.NotFound);
        
        entity.Name = request.Name;

        await dbContext.SaveChangesAsync(cancellationToken);
        
        return entity.Adapt<ServiceTypeDto>();
    }
}
