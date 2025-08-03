using Domain.Entities.Vehicles;
using Domain.Enums;

namespace DomainTests.Vehicles;

public class VehicleTests
{
    private const string Brand = "Audi";
    private const string Model = "A4";
    private const PowerType PowerType = Domain.Enums.PowerType.Gasoline;
    private readonly int? _manufacturedYear = 2010;
    private readonly Guid _userId = Guid.NewGuid();
    private const VehicleType VehicleType = Domain.Enums.VehicleType.Car;
    private const string VIN = "1HGBH41JXMN109186";

    [Fact]
    public void Constructor_ValidProperties_CreatesVehicleWithCorrectData()
    {
        var vehicle = new Vehicle 
        { 
            Brand = Brand, 
            Model = Model, 
            PowerType = PowerType,
            ManufacturedYear = _manufacturedYear, 
            UserId = _userId 
        };

        vehicle.Brand.ShouldBe(Brand);
        vehicle.Model.ShouldBe(Model);
        vehicle.PowerType.ShouldBe(PowerType);
        vehicle.ManufacturedYear.ShouldBe(_manufacturedYear);
        vehicle.UserId.ShouldBe(_userId);
    }

    [Fact]
    public void Constructor_ValidPropertiesAndNullManufacturedYear_CreatesVehicleWithCorrectData()
    {
        var vehicle = new Vehicle 
        { 
            Brand = Brand, 
            Model = Model, 
            PowerType = PowerType,
            ManufacturedYear = null, 
            UserId = _userId 
        };

        vehicle.Brand.ShouldBe(Brand);
        vehicle.Model.ShouldBe(Model);
        vehicle.PowerType.ShouldBe(PowerType);
        vehicle.ManufacturedYear.ShouldBeNull();
        vehicle.UserId.ShouldBe(_userId);
    }

    [Fact]
    public void Constructor_ValidPropertiesWithOptionalFields_CreatesVehicleWithCorrectData()
    {
        var vehicle = new Vehicle 
        { 
            Brand = Brand, 
            Model = Model, 
            PowerType = PowerType,
            ManufacturedYear = _manufacturedYear,
            Type = VehicleType,
            VIN = VIN,
            UserId = _userId 
        };

        vehicle.Brand.ShouldBe(Brand);
        vehicle.Model.ShouldBe(Model);
        vehicle.PowerType.ShouldBe(PowerType);
        vehicle.ManufacturedYear.ShouldBe(_manufacturedYear);
        vehicle.Type.ShouldBe(VehicleType);
        vehicle.VIN.ShouldBe(VIN);
        vehicle.UserId.ShouldBe(_userId);
    }

    [Fact]
    public void Setters_PropertiesUpdated_UpdatesVehicleDataCorrectly()
    {
        var vehicle = new Vehicle 
        { 
            Brand = Brand, 
            Model = Model, 
            PowerType = PowerType,
            ManufacturedYear = _manufacturedYear, 
            UserId = _userId 
        };

        const string newVehicleBrand = "BMW";
        const string newVehicleModel = "Series 3";
        const PowerType newPowerType = PowerType.Diesel;
        const int newVehicleManufacturedYear = 2015;
        const VehicleType newVehicleType = VehicleType.Truck;
        const string newVIN = "2HGBH41JXMN109187";
        var newVehicleUserId = Guid.NewGuid();

        vehicle.Brand = newVehicleBrand;
        vehicle.Model = newVehicleModel;
        vehicle.PowerType = newPowerType;
        vehicle.ManufacturedYear = newVehicleManufacturedYear;
        vehicle.Type = newVehicleType;
        vehicle.VIN = newVIN;
        vehicle.UserId = newVehicleUserId;

        vehicle.Brand.ShouldBe(newVehicleBrand);
        vehicle.Model.ShouldBe(newVehicleModel);
        vehicle.PowerType.ShouldBe(newPowerType);
        vehicle.ManufacturedYear.ShouldBe(newVehicleManufacturedYear);
        vehicle.Type.ShouldBe(newVehicleType);
        vehicle.VIN.ShouldBe(newVIN);
        vehicle.UserId.ShouldBe(newVehicleUserId);
    }

    [Theory]
    [InlineData(PowerType.Gasoline)]
    [InlineData(PowerType.Diesel)]
    [InlineData(PowerType.Hybrid)]
    [InlineData(PowerType.PlugInHybrid)]
    [InlineData(PowerType.Electric)]
    public void PowerType_ValidPowerTypes_SetsPowerTypeCorrectly(PowerType powerType)
    {
        var vehicle = new Vehicle 
        { 
            Brand = Brand, 
            Model = Model, 
            PowerType = powerType,
            UserId = _userId 
        };

        vehicle.PowerType.ShouldBe(powerType);
    }

    [Theory]
    [InlineData(VehicleType.Bus)]
    [InlineData(VehicleType.Car)]
    [InlineData(VehicleType.Motorbike)]
    [InlineData(VehicleType.Truck)]
    public void Type_ValidVehicleTypes_SetsTypeCorrectly(VehicleType vehicleType)
    {
        var vehicle = new Vehicle 
        { 
            Brand = Brand, 
            Model = Model, 
            PowerType = PowerType,
            Type = vehicleType,
            UserId = _userId 
        };

        vehicle.Type.ShouldBe(vehicleType);
    }
}