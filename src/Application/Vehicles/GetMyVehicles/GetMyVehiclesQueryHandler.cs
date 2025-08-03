using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Mapster;

namespace Application.Vehicles.GetMyVehicles;

internal sealed class GetMyVehiclesQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetMyVehiclesQuery, PagedList<VehicleDto>>
{
    public async Task<Result<PagedList<VehicleDto>>> Handle(GetMyVehiclesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
            return Result.Failure<PagedList<VehicleDto>>(VehicleErrors.Unauthorized);

        var vehiclesQuery = context.Vehicles.AsQueryable();

        vehiclesQuery = vehiclesQuery.Where(v => v.UserId == request.UserId);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            vehiclesQuery = vehiclesQuery.Where(v =>
                v.Brand.Contains(request.SearchTerm) ||
                v.Model.Contains(request.SearchTerm));
        }

        var vehiclesDtoQuery = vehiclesQuery
            .OrderBy(v => v.CreatedDate)
            .ProjectToType<VehicleDto>();
        
        var vehiclesDto = await PagedList<VehicleDto>.CreateAsync(
            vehiclesDtoQuery,
            request.Page,
            request.PageSize);

        return Result.Success(vehiclesDto);
    }
}