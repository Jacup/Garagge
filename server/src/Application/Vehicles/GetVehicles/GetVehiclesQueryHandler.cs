using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Core;
using Domain.Entities.Vehicles;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Application.Vehicles.GetVehicles;

internal sealed class GetVehiclesQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetVehiclesQuery, PagedList<VehicleDto>>
{
    public async Task<Result<PagedList<VehicleDto>>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
            return Result.Failure<PagedList<VehicleDto>>(VehicleErrors.Unauthorized);

        var vehiclesQuery = context.Vehicles
            .AsNoTracking()
            .Where(v => v.UserId == userId);

        vehiclesQuery = ApplyFiltering(request, vehiclesQuery);

        vehiclesQuery = vehiclesQuery.OrderBy(v => v.CreatedDate);

        var vehiclesDtoQuery = vehiclesQuery
            .Include(v => v.VehicleEnergyTypes)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                CreatedDate = v.CreatedDate,
                UpdatedDate = v.UpdatedDate,
                Brand = v.Brand,
                Model = v.Model,
                EngineType = v.EngineType,
                ManufacturedYear = v.ManufacturedYear,
                Type = v.Type,
                VIN = v.VIN,
                UserId = v.UserId,
                AllowedEnergyTypes = v.AllowedEnergyTypes
            });

        var vehiclesDto = await PagedList<VehicleDto>.CreateAsync(
            vehiclesDtoQuery,
            request.Page,
            request.PageSize);

        return Result.Success(vehiclesDto);
    }

    private static IQueryable<Vehicle> ApplyFiltering(GetVehiclesQuery request, IQueryable<Vehicle> vehiclesQuery)
    {
        if (string.IsNullOrEmpty(request.SearchTerm))
            return vehiclesQuery;

        var searchTerm = request.SearchTerm.ToLower();

#pragma warning disable CA1862 // EF Core translates ToLower to SQL LOWER function
        vehiclesQuery = vehiclesQuery.Where(v =>
            v.Brand.ToLower().Contains(searchTerm.ToLower()) ||
            v.Model.ToLower().Contains(searchTerm.ToLower()));
#pragma warning restore CA1862

        return vehiclesQuery;
    }
}