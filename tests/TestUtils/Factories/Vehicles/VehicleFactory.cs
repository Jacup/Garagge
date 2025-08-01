using Domain.Entities.Vehicles;

namespace TestUtils.Factories.Vehicles;

public static class VehicleFactory
{
    public static Vehicle Create(
        string brand,
        string model,
        DateOnly manufacturedYear,
        Guid userId)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = brand,
            Model = model,
            ManufacturedYear = manufacturedYear,
            UserId = userId,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        // vehicle.Raise(new VehicleCreatedDomainEvent(vehicle.Id));

        return vehicle;
    }

    public static Guid AudiId { get; } = Guid.NewGuid();
    public static Guid AudiUserId { get; } = Guid.NewGuid();

    public static Vehicle CreateDefaultAudi() => new()
    {
        Id = AudiId,
        Brand = "Audi",
        Model = "A4",
        ManufacturedYear = new DateOnly(2010, 01, 20),
        UserId = AudiUserId,
    };

    public static Vehicle CreateDefaultBmw() => new()
    {
        Id = Guid.NewGuid(),
        Brand = "BMW",
        Model = "3 Series",
        ManufacturedYear = new DateOnly(2010, 01, 20),
        UserId = Guid.NewGuid(),
    };
}