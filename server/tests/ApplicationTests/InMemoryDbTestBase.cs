using Application.Abstractions.Authentication;
using ApplicationTests.Helpers;
using Domain.Entities.Vehicles;
using Domain.Enums;
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
            vehicle.VehicleEnergyTypes.Add(new VehicleEnergyType
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicle.Id,
                EnergyType = energyType
            });
        }

        Context.Vehicles.Add(vehicle);
        await Context.SaveChangesAsync();
        return vehicle;
    }
}