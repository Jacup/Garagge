using Infrastructure.DAL;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using TestUtils.Factories.Vehicles;

namespace ApplicationTests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(Mock<IPublisher> publisherMock)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options, publisherMock.Object);
        context.Database.EnsureCreated();

        return context;
    }

    public static void Destroy(ApplicationDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
    
    public static ApplicationDbContext SeedDefaultVehicles(this ApplicationDbContext context)
    {
        context.Vehicles.Add(VehicleFactory.CreateDefaultAudi());
        context.Vehicles.Add(VehicleFactory.CreateDefaultBmw());
        
        context.SaveChanges();
        
        return context;
    }
}