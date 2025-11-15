using Application.Abstractions.Authentication;
using ApplicationTests.Helpers;
using Domain.Entities.Services;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Domain.Enums.Services;
using Infrastructure.DAL;
using MediatR;
using Moq;

namespace ApplicationTests;

public abstract class InMemoryDbTestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<IPublisher> PublisherMock;
    protected readonly Mock<IUserContext> UserContextMock = new();

    protected readonly Guid AuthorizedUserId = Guid.NewGuid();

    protected InMemoryDbTestBase()
    {
        PublisherMock = new Mock<IPublisher>();
        Context = TestDbContextFactory.Create(PublisherMock);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(Context);
    }

    protected void SetupAuthorizedUser()
    {
        UserContextMock
            .Setup(o => o.UserId)
            .Returns(AuthorizedUserId);
    }

    protected void SetupUnauthenticatedUser()
    {
        UserContextMock
            .Setup(o => o.UserId)
            .Returns(Guid.Empty);
    }

    protected async Task<Vehicle> CreateVehicleInDb(EnergyType[] supportedEnergyTypes, Guid? userId = null)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            ManufacturedYear = 2020,
            Type = VehicleType.Car,
            UserId = userId ?? AuthorizedUserId,
        };

        foreach (var energyType in supportedEnergyTypes)
        {
            vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType { Id = Guid.NewGuid(), VehicleId = vehicle.Id, EnergyType = energyType });
        }

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }

    protected async Task<ServiceType> CreateServiceTypeInDb(string name = "Test Service Type")
    {
        var serviceType = new ServiceType { Id = Guid.NewGuid(), Name = name };

        Context.ServiceTypes.Add(serviceType);
        await Context.SaveChangesAsync();
        return serviceType;
    }

    protected async Task<ServiceRecord> CreateServiceRecordInDb(
        Guid vehicleId,
        Guid serviceTypeId,
        string title = "Test Service Record",
        DateTime? serviceDate = null,
        int? mileage = null,
        decimal? manualCost = null,
        string? notes = null)
    {
        var serviceRecord = new ServiceRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            TypeId = serviceTypeId,
            Title = title,
            ServiceDate = serviceDate ?? DateTime.UtcNow.Date,
            Notes = notes,
            Mileage = mileage,
            ManualCost = manualCost
        };

        Context.ServiceRecords.Add(serviceRecord);
        await Context.SaveChangesAsync();
        return serviceRecord;
    }

    protected async Task<ServiceItem> CreateServiceItemInDb(
        Guid serviceRecordId,
        string name = "Test Item",
        ServiceItemType type = ServiceItemType.Part,
        decimal unitPrice = 100.00m,
        decimal quantity = 1,
        string? partNumber = null,
        string? notes = null)
    {
        var serviceItem = new ServiceItem
        {
            Id = Guid.NewGuid(),
            ServiceRecordId = serviceRecordId,
            Name = name,
            Type = type,
            UnitPrice = unitPrice,
            Quantity = quantity,
            PartNumber = partNumber,
            Notes = notes
        };

        Context.ServiceItems.Add(serviceItem);
        await Context.SaveChangesAsync();
        return serviceItem;
    }
}