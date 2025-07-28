using Application.Vehicles;
using Domain.Entities.Vehicles;
using Mapster;

namespace ApplicationTests.Vehicles;

public class VehicleDtoTests
{
    private const string Brand = "Audi";
    private const string Model = "A4";
    readonly DateOnly _manufacturedYear = new(2010, 01, 30);
    readonly Guid _userId = Guid.NewGuid();
    
    [Fact]
    public void Adapt_ValidData_ShouldConvertToDtoWithMatchingValues()
    {
        var vehicle = new Vehicle { Brand = Brand, Model = Model, ManufacturedYear = _manufacturedYear, UserId = _userId };

        var dto = vehicle.Adapt<VehicleDto>();
        
        dto.Brand.ShouldBe(Brand);
        dto.Model.ShouldBe(Model);
        dto.ManufacturedYear.ShouldBe(_manufacturedYear);
        dto.UserId.ShouldBe(_userId);
    }
}