using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Application.Features.ServiceRecords;
using Domain.Entities.Services;
using Mapster;

namespace Application.Features.ServiceTypes.Create;

internal sealed class CreateServiceTypeCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<CreateServiceTypeCommand, ServiceTypeDto>
{
    public async Task<Result<ServiceTypeDto>> Handle(CreateServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var newEntity = new ServiceType
        {
            Id = Guid.NewGuid(),
            Name = request.Name
        };
        
        await dbContext.ServiceTypes.AddAsync(newEntity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return newEntity.Adapt<ServiceTypeDto>();
    }
}
