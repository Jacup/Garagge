using Application.Vehicles;
using Domain.Enums;

namespace ApplicationTests.Vehicles;

public class VehicleDtoTests
{
    [Fact]
    public void VehicleDto_WhenCreatedWithValidData_ShouldSetPropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var brand = "Audi";
        var model = "A4";
        var powerType = EngineType.Fuel;
        
        var manufacturedYear = 2010;
        var type = VehicleType.Car;
        var vin = "1HGBH41JXMN109186";
        
        var createdDate = DateTime.UtcNow;
        var updatedDate = DateTime.UtcNow;

        var dto = new VehicleDto
        {
            Id = id,
            UserId = userId,
            Brand = brand,
            Model = model,
            EngineType = powerType,
            ManufacturedYear = manufacturedYear,
            Type = type,
            VIN = vin,
            CreatedDate = createdDate,
            UpdatedDate = updatedDate
        };

        dto.Id.ShouldBe(id);
        dto.UserId.ShouldBe(userId);
        dto.Brand.ShouldBe(brand);
        dto.Model.ShouldBe(model);
        dto.EngineType.ShouldBe(powerType);
        dto.ManufacturedYear.ShouldBe(manufacturedYear);
        dto.Type.ShouldBe(type);
        dto.VIN.ShouldBe(vin);
        dto.CreatedDate.ShouldBe(createdDate);
        dto.UpdatedDate.ShouldBe(updatedDate);
    }
    
    [Fact]
    public void VehicleDto_MinimumData_ShouldBeNullable()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var brand = "Audi";
        var powerType = EngineType.Fuel;
        var model = "A4";
        var createdDate = DateTime.UtcNow;
        var updatedDate = DateTime.UtcNow;

        var dto = new VehicleDto
        {
            Id = id,
            UserId = userId,
            Brand = brand,
            Model = model,
            EngineType = powerType,
            
            ManufacturedYear = null,
            Type = null,
            VIN = null,
            
            CreatedDate = createdDate,
            UpdatedDate = updatedDate
        };

        dto.Id.ShouldBe(id);
        dto.UserId.ShouldBe(userId);
        dto.Brand.ShouldBe(brand);
        dto.Model.ShouldBe(model);
        dto.EngineType.ShouldBe(powerType);
        dto.ManufacturedYear.ShouldBeNull();
        dto.Type.ShouldBeNull();
        dto.VIN.ShouldBeNull();
        dto.CreatedDate.ShouldBe(createdDate);
        dto.UpdatedDate.ShouldBe(updatedDate);
    }
}
