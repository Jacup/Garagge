using Domain.Entities.Vehicles;

namespace DomainTests.Vehicles;

public class VehicleTests
{
    private const string Brand = "Audi";
    private const string Model = "A4";
    readonly int? _manufacturedYear = 2010;
    readonly Guid _userId = Guid.NewGuid();
    
    [Fact]
    public void Constructor_ValidProperties_CreatesVehicleWithCorrectData()
    {
        var vehicle = new Vehicle { Brand = Brand, Model = Model, ManufacturedYear = _manufacturedYear, UserId = _userId };

        vehicle.Brand.ShouldBe(Brand);
        vehicle.Model.ShouldBe(Model);
        vehicle.ManufacturedYear.ShouldBe(_manufacturedYear);
        vehicle.UserId.ShouldBe(_userId);
    }
    
    [Fact]
    public void Constructor_ValidPropertiesAndNullManufacturedYear_CreatesVehicleWithCorrectData()
    {
        var vehicle = new Vehicle { Brand = Brand, Model = Model, ManufacturedYear = null, UserId = _userId };

        vehicle.Brand.ShouldBe(Brand);
        vehicle.Model.ShouldBe(Model);
        vehicle.ManufacturedYear.ShouldBeNull();
        vehicle.UserId.ShouldBe(_userId);
    }
    
    [Fact]
    public void Setters_PropertiesUpdated_UpdatesUserDataCorrectly()
    {
        var vehicle = new Vehicle { Brand = Brand, Model = Model, ManufacturedYear = _manufacturedYear, UserId = _userId };

        const string newVehicleBrand = "BMW";
        const string newVehicleModel = "Series 3";
        const int newVehicleManufacturedYear = 2015;
        var newVehicleUserId = Guid.AllBitsSet;
        
        vehicle.Brand = newVehicleBrand;
        vehicle.Model = newVehicleModel;
        vehicle.ManufacturedYear = newVehicleManufacturedYear;
        vehicle.UserId = newVehicleUserId;
        
        vehicle.Brand.ShouldBe(newVehicleBrand);
        vehicle.Model.ShouldBe(newVehicleModel);
        vehicle.ManufacturedYear.ShouldBe(newVehicleManufacturedYear);
        vehicle.UserId.ShouldBe(newVehicleUserId);
    }
}