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

        var vehiclesQuery = context.Vehicles.Where(v => v.UserId == request.UserId);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            
            vehiclesQuery = vehiclesQuery.Where(v =>
                v.Brand.ToLower().Contains(searchTerm) ||
                v.Model.ToLower().Contains(searchTerm));
        }
        
        vehiclesQuery = vehiclesQuery.OrderBy(v => v.CreatedDate);

        var vehiclesDtoQuery = vehiclesQuery.ProjectToType<VehicleDto>();
        
        var vehiclesDto = await PagedList<VehicleDto>.CreateAsync(
            vehiclesDtoQuery,
            request.Page,
            request.PageSize);

        return Result.Success(vehiclesDto);
    }
}