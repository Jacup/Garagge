using Domain.Entities.Vehicles;
using Domain.Enums;

namespace DomainTests.Vehicles;

public class VehicleTests
{
    private const string Brand = "Audi";
    private const string Model = "A4";
    private const EngineType EngineType = Domain.Enums.EngineType.Fuel;
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
            EngineType = EngineType,
            ManufacturedYear = _manufacturedYear, 
            UserId = _userId 
        };

        vehicle.Brand.ShouldBe(Brand);
        vehicle.Model.ShouldBe(Model);
        vehicle.EngineType.ShouldBe(EngineType);
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
            EngineType = EngineType,
            ManufacturedYear = null, 
            UserId = _userId 
        };

        vehicle.Brand.ShouldBe(Brand);
        vehicle.Model.ShouldBe(Model);
        vehicle.EngineType.ShouldBe(EngineType);
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
            EngineType = EngineType,
            ManufacturedYear = _manufacturedYear,
            Type = VehicleType,
            VIN = VIN,
            UserId = _userId 
        };

        vehicle.Brand.ShouldBe(Brand);
        vehicle.Model.ShouldBe(Model);
        vehicle.EngineType.ShouldBe(EngineType);
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
            EngineType = EngineType,
            ManufacturedYear = _manufacturedYear, 
            UserId = _userId 
        };

        const string newVehicleBrand = "BMW";
        const string newVehicleModel = "Series 3";
        const EngineType newPowerType = EngineType.Electric;
        const int newVehicleManufacturedYear = 2015;
        const VehicleType newVehicleType = VehicleType.Truck;
        const string newVIN = "2HGBH41JXMN109187";
        var newVehicleUserId = Guid.NewGuid();

        vehicle.Brand = newVehicleBrand;
        vehicle.Model = newVehicleModel;
        vehicle.EngineType = newPowerType;
        vehicle.ManufacturedYear = newVehicleManufacturedYear;
        vehicle.Type = newVehicleType;
        vehicle.VIN = newVIN;
        vehicle.UserId = newVehicleUserId;

        vehicle.Brand.ShouldBe(newVehicleBrand);
        vehicle.Model.ShouldBe(newVehicleModel);
        vehicle.EngineType.ShouldBe(newPowerType);
        vehicle.ManufacturedYear.ShouldBe(newVehicleManufacturedYear);
        vehicle.Type.ShouldBe(newVehicleType);
        vehicle.VIN.ShouldBe(newVIN);
        vehicle.UserId.ShouldBe(newVehicleUserId);
    }

    [Theory]
    [InlineData(EngineType.Fuel)]
    [InlineData(EngineType.Hybrid)]
    [InlineData(EngineType.PlugInHybrid)]
    [InlineData(EngineType.Electric)]
    [InlineData(EngineType.Hydrogen)]
    public void PowerType_ValidPowerTypes_SetsPowerTypeCorrectly(EngineType powerType)
    {
        var vehicle = new Vehicle 
        { 
            Brand = Brand, 
            Model = Model, 
            EngineType = powerType,
            UserId = _userId 
        };

        vehicle.EngineType.ShouldBe(powerType);
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
            EngineType = EngineType,
            Type = vehicleType,
            UserId = _userId 
        };

        vehicle.Type.ShouldBe(vehicleType);
    }

    [Fact]
    public void VehicleEnergyTypes_InitializedAsEmpty_ReturnsEmptyCollection()
    {
        var vehicle = new Vehicle 
        { 
            Brand = Brand, 
            Model = Model, 
            EngineType = EngineType,
            UserId = _userId 
        };

        vehicle.VehicleEnergyTypes.ShouldBeEmpty();
    }

    [Fact]
    public void EnergyEntries_InitializedAsEmpty_ReturnsEmptyCollection()
    {
        var vehicle = new Vehicle 
        { 
            Brand = Brand, 
            Model = Model, 
            EngineType = EngineType,
            UserId = _userId 
        };

        vehicle.EnergyEntries.ShouldBeEmpty();
    }

    [Fact]
    public void AllowedEnergyTypes_WithVehicleEnergyTypes_ReturnsCorrectEnergyTypes()
    {
        var vehicle = new Vehicle 
        { 
            Brand = Brand, 
            Model = Model, 
            EngineType = EngineType,
            UserId = _userId 
        };

        var vehicleEnergyType1 = new VehicleEnergyType
        {
            VehicleId = vehicle.Id,
            EnergyType = EnergyType.Gasoline
        };

        var vehicleEnergyType2 = new VehicleEnergyType
        {
            VehicleId = vehicle.Id,
            EnergyType = EnergyType.Diesel
        };

        vehicle.VehicleEnergyTypes.Add(vehicleEnergyType1);
        vehicle.VehicleEnergyTypes.Add(vehicleEnergyType2);

        var allowedEnergyTypes = vehicle.AllowedEnergyTypes.ToList();
        allowedEnergyTypes.ShouldContain(EnergyType.Gasoline);
        allowedEnergyTypes.ShouldContain(EnergyType.Diesel);
        allowedEnergyTypes.Count.ShouldBe(2);
    }
}