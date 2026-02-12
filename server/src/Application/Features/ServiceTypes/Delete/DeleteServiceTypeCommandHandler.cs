using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ServiceTypes.Delete;

internal sealed class DeleteServiceTypeCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<DeleteServiceTypeCommand>
{
    public async Task<Result> Handle(DeleteServiceTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ServiceTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(st => st.Id == request.Id, cancellationToken);
        
        if (entity == null)
            return Result.Failure(ServiceTypeErrors.NotFound);
        
        var recordsWithType = await dbContext.ServiceRecords
            .Where(sr => sr.TypeId == request.Id)
            .CountAsync(cancellationToken);

        if (recordsWithType != 0)
            return Result.Failure(ServiceTypeErrors.ServiceRecordsExists);

        dbContext.ServiceTypes.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
