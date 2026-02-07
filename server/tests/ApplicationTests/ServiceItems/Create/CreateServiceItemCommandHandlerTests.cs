using Application.ServiceItems;
using Application.ServiceItems.Create;
using Application.ServiceRecords;
using Domain.Enums;
using Domain.Enums.Services;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.ServiceItems.Create;

public class CreateServiceItemCommandHandlerTests : InMemoryDbTestBase
{
    private readonly CreateServiceItemCommandHandler _handler;

    public CreateServiceItemCommandHandlerTests()
    {
        _handler = new CreateServiceItemCommandHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithServiceItemDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new CreateServiceItemCommand(
            serviceRecord.Id,
            "Engine Oil 5W-30",
            ServiceItemType.Part,
            45.00m,
            5,
            "12345",
            "Premium synthetic oil");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Engine Oil 5W-30");
        result.Value.Type.ShouldBe(ServiceItemType.Part);
        result.Value.UnitPrice.ShouldBe(45.00m);
        result.Value.Quantity.ShouldBe(5);
        result.Value.TotalPrice.ShouldBe(225.00m);
        result.Value.PartNumber.ShouldBe("12345");
        result.Value.Notes.ShouldBe("Premium synthetic oil");
        result.Value.ServiceRecordId.ShouldBe(serviceRecord.Id);

        var addedEntity = await Context.ServiceItems
            .FirstOrDefaultAsync(si => si.Id == result.Value.Id);

        addedEntity.ShouldNotBeNull();
        addedEntity.Name.ShouldBe("Engine Oil 5W-30");
        addedEntity.Type.ShouldBe(ServiceItemType.Part);
        addedEntity.UnitPrice.ShouldBe(45.00m);
        addedEntity.Quantity.ShouldBe(5);
        addedEntity.PartNumber.ShouldBe("12345");
        addedEntity.Notes.ShouldBe("Premium synthetic oil");
        addedEntity.ServiceRecordId.ShouldBe(serviceRecord.Id);
    }

    [Fact]
    public async Task Handle_ValidCommandWithMinimalData_ReturnsSuccessWithServiceItemDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new CreateServiceItemCommand(
            serviceRecord.Id,
            "Labor",
            ServiceItemType.Labor,
            50.00m,
            1,
            null,
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Labor");
        result.Value.Type.ShouldBe(ServiceItemType.Labor);
        result.Value.UnitPrice.ShouldBe(50.00m);
        result.Value.Quantity.ShouldBe(1);
        result.Value.TotalPrice.ShouldBe(50.00m);
        result.Value.PartNumber.ShouldBeNull();
        result.Value.Notes.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ServiceRecordNotFound_ReturnsServiceRecordNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var nonExistentServiceRecordId = Guid.NewGuid();

        var command = new CreateServiceItemCommand(
            nonExistentServiceRecordId,
            "Test Item",
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ServiceRecordNotOwnedByUser_ReturnsNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline], otherUserId);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new CreateServiceItemCommand(
            serviceRecord.Id,
            "Test Item",
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ValidCommand_GeneratesNewGuidForServiceItemId()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new CreateServiceItemCommand(
            serviceRecord.Id,
            "Test Item",
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(ServiceItemType.Part)]
    [InlineData(ServiceItemType.Labor)]
    [InlineData(ServiceItemType.Tax)]
    [InlineData(ServiceItemType.Other)]
    public async Task Handle_AllServiceItemTypes_CreatesSuccessfully(ServiceItemType itemType)
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Service");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new CreateServiceItemCommand(

            serviceRecord.Id,
            $"Test {itemType}",
            itemType,
            10.00m,
            1,
            null,
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Type.ShouldBe(itemType);
    }

    [Fact]
    public async Task Handle_WithDecimalQuantity_CalculatesTotalPriceCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Service");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new CreateServiceItemCommand(

            serviceRecord.Id,
            "Labor",
            ServiceItemType.Labor,
            100.00m,
            2.5m,
            null,
            "2.5 hours");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Quantity.ShouldBe(2.5m);
        result.Value.TotalPrice.ShouldBe(250.00m);
    }

    [Fact]
    public async Task Handle_WithZeroQuantity_CreateSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Service");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new CreateServiceItemCommand(

            serviceRecord.Id,
            "Quote Item",
            ServiceItemType.Part,
            100.00m,
            0,
            "QUOTE123",
            "For quote purposes");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Quantity.ShouldBe(0);
        result.Value.TotalPrice.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WithZeroUnitPrice_CreateSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Service");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new CreateServiceItemCommand(

            serviceRecord.Id,
            "Free Item",
            ServiceItemType.Part,
            0,
            1,
            null,
            "Warranty covered");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.UnitPrice.ShouldBe(0);
        result.Value.TotalPrice.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WithLongPartNumber_CreatesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Service");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var longPartNumber = new string('A', 50);
        var command = new CreateServiceItemCommand(

            serviceRecord.Id,
            "Part",
            ServiceItemType.Part,
            10.00m,
            1,
            longPartNumber,
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.PartNumber.ShouldBe(longPartNumber);
    }

    [Fact]
    public async Task Handle_WithLongNotes_CreatesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Service");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var longNotes = new string('A', 500);
        var command = new CreateServiceItemCommand(

            serviceRecord.Id,
            "Item",
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            longNotes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Notes.ShouldBe(longNotes);
    }

    [Fact]
    public async Task Handle_MultipleItemsForSameServiceRecord_CreatesAllSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Service");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command1 = new CreateServiceItemCommand(

            serviceRecord.Id,
            "Part 1",
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            null);

        var command2 = new CreateServiceItemCommand(

            serviceRecord.Id,
            "Part 2",
            ServiceItemType.Part,
            20.00m,
            2,
            null,
            null);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();
        result1.Value.Id.ShouldNotBe(result2.Value.Id);

        var itemsInDb = await Context.ServiceItems
            .Where(si => si.ServiceRecordId == serviceRecord.Id)
            .ToListAsync();
        itemsInDb.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WithLargePriceAndQuantity_CalculatesCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Service");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new CreateServiceItemCommand(

            serviceRecord.Id,
            "Expensive Part",
            ServiceItemType.Part,
            9999.99m,
            100,
            null,
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalPrice.ShouldBe(999999.00m);
    }

    [Fact]
    public async Task Handle_PersistsCorrectTimestamps()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Service");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        var command = new CreateServiceItemCommand(

            serviceRecord.Id,
            "Test Item",
            ServiceItemType.Part,
            10.00m,
            1,
            null,
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.CreatedDate.ShouldBeGreaterThanOrEqualTo(beforeCreation);
        result.Value.CreatedDate.ShouldBeLessThanOrEqualTo(afterCreation);
        result.Value.UpdatedDate.ShouldBeGreaterThanOrEqualTo(beforeCreation);
        result.Value.UpdatedDate.ShouldBeLessThanOrEqualTo(afterCreation);
    }
}
