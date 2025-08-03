using Application.Vehicles;

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
        var manufacturedYear = 2010;
        var createdDate = DateTime.UtcNow;
        var updatedDate = DateTime.UtcNow;

        var dto = new VehicleDto
        {
            Id = id,
            UserId = userId,
            Brand = brand,
            Model = model,
            ManufacturedYear = manufacturedYear,
            CreatedDate = createdDate,
            UpdatedDate = updatedDate
        };

        dto.Id.ShouldBe(id);
        dto.UserId.ShouldBe(userId);
        dto.Brand.ShouldBe(brand);
        dto.Model.ShouldBe(model);
        dto.ManufacturedYear.ShouldBe(manufacturedYear);
        dto.CreatedDate.ShouldBe(createdDate);
        dto.UpdatedDate.ShouldBe(updatedDate);
    }
    
    [Fact]
    public void VehicleDto_ManufacturedYearEmpty_ShouldBeNullable()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var brand = "Audi";
        var model = "A4";
        var createdDate = DateTime.UtcNow;
        var updatedDate = DateTime.UtcNow;

        var dto = new VehicleDto
        {
            Id = id,
            UserId = userId,
            Brand = brand,
            Model = model,
            ManufacturedYear = null,
            CreatedDate = createdDate,
            UpdatedDate = updatedDate
        };

        dto.Id.ShouldBe(id);
        dto.UserId.ShouldBe(userId);
        dto.Brand.ShouldBe(brand);
        dto.Model.ShouldBe(model);
        dto.ManufacturedYear.ShouldBeNull();
        dto.CreatedDate.ShouldBe(createdDate);
        dto.UpdatedDate.ShouldBe(updatedDate);
    }
}
