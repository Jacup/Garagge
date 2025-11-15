using Application.ServiceItems;
using Application.ServiceItems.Update;
using Application.ServiceRecords;
using Domain.Entities.Services;
using Domain.Enums;
using Domain.Enums.Services;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.ServiceItems.Update;

public class UpdateServiceItemCommandHandlerTests : InMemoryDbTestBase
{
    private readonly UpdateServiceItemCommandHandler _handler;

    public UpdateServiceItemCommandHandlerTests()
    {
        _handler = new UpdateServiceItemCommandHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithUpdatedServiceItemDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id, "Old Name");

        var command = new UpdateServiceItemCommand(
            serviceItem.Id,
            serviceRecord.Id,
            "New Engine Oil 5W-30",
            ServiceItemType.Labor,
            75.00m,
            3,
            "NEW-12345",
            "Updated premium synthetic oil");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(serviceItem.Id);
        result.Value.Name.ShouldBe("New Engine Oil 5W-30");
        result.Value.Type.ShouldBe(ServiceItemType.Labor);
        result.Value.UnitPrice.ShouldBe(75.00m);
        result.Value.Quantity.ShouldBe(3);
        result.Value.TotalPrice.ShouldBe(225.00m);
        result.Value.PartNumber.ShouldBe("NEW-12345");
        result.Value.Notes.ShouldBe("Updated premium synthetic oil");
        result.Value.ServiceRecordId.ShouldBe(serviceRecord.Id);

        var updatedEntity = await Context.ServiceItems.FirstOrDefaultAsync(si => si.Id == serviceItem.Id);

        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("New Engine Oil 5W-30");
        updatedEntity.Type.ShouldBe(ServiceItemType.Labor);
        updatedEntity.UnitPrice.ShouldBe(75.00m);
        updatedEntity.Quantity.ShouldBe(3);
        updatedEntity.PartNumber.ShouldBe("NEW-12345");
        updatedEntity.Notes.ShouldBe("Updated premium synthetic oil");
    }

    [Fact]
    public async Task Handle_ValidCommandWithNullValues_ReturnsSuccessWithUpdatedServiceItemDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(
            serviceRecord.Id,
            "Old Name",
            ServiceItemType.Part,
            100.00m,
            1,
            "OLD-PART",
            "Old notes");

        var command = new UpdateServiceItemCommand(
            serviceItem.Id,
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

        var updatedEntity = await Context.ServiceItems.FirstOrDefaultAsync(si => si.Id == serviceItem.Id);

        updatedEntity.ShouldNotBeNull();
        updatedEntity.PartNumber.ShouldBeNull();
        updatedEntity.Notes.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ServiceItemNotFound_ReturnsServiceItemNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var nonExistentServiceItemId = Guid.NewGuid();

        var command = new UpdateServiceItemCommand(
            nonExistentServiceItemId,
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
        result.Error.ShouldBe(ServiceItemsErrors.NotFound(nonExistentServiceItemId));
    }

    [Fact]
    public async Task Handle_ServiceRecordNotFound_ReturnsServiceItemNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id);
        var nonExistentServiceRecordId = Guid.NewGuid();

        var command = new UpdateServiceItemCommand(
            serviceItem.Id,
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
        result.Error.ShouldBe(ServiceItemsErrors.NotFound(serviceItem.Id));
    }

    [Fact]
    public async Task Handle_ServiceItemBelongsToDifferentServiceRecord_ReturnsServiceItemNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord1 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceRecord2 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord1.Id);

        var command = new UpdateServiceItemCommand(
            serviceItem.Id,
            serviceRecord2.Id,
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
        result.Error.ShouldBe(ServiceItemsErrors.NotFound(serviceItem.Id));
    }

    [Fact]
    public async Task Handle_ServiceRecordNotOwnedByUser_ReturnsUnauthorizedError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline], otherUserId);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id);

        var command = new UpdateServiceItemCommand(
            serviceItem.Id,
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
        result.Error.ShouldBe(ServiceItemsErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_UpdateOnlyName_KeepsOtherFieldsIntact()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(
            serviceRecord.Id,
            "Original Name",
            ServiceItemType.Part,
            100.00m,
            1,
            "PART-123",
            "Original notes");

        var command = new UpdateServiceItemCommand(
            serviceItem.Id,
            serviceRecord.Id,
            "Updated Name Only",
            ServiceItemType.Part,
            100.00m,
            1,
            "PART-123",
            "Original notes");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Updated Name Only");
        result.Value.Type.ShouldBe(ServiceItemType.Part);
        result.Value.PartNumber.ShouldBe("PART-123");
        result.Value.Notes.ShouldBe("Original notes");
    }

    [Fact]
    public async Task Handle_UpdateAllFields_UpdatesAllFieldsCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(
            serviceRecord.Id,
            "Original Name",
            ServiceItemType.Part,
            100.00m,
            1,
            "OLD-123",
            "Old notes");

        var command = new UpdateServiceItemCommand(
            serviceItem.Id,
            serviceRecord.Id,
            "Completely New Name",
            ServiceItemType.Labor,
            99.99m,
            7,
            "NEW-999",
            "Brand new notes");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        var updatedEntity = await Context.ServiceItems.FirstOrDefaultAsync(si => si.Id == serviceItem.Id);

        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("Completely New Name");
        updatedEntity.Type.ShouldBe(ServiceItemType.Labor);
        updatedEntity.UnitPrice.ShouldBe(99.99m);
        updatedEntity.Quantity.ShouldBe(7);
        updatedEntity.TotalPrice.ShouldBe(699.93m);
        updatedEntity.PartNumber.ShouldBe("NEW-999");
        updatedEntity.Notes.ShouldBe("Brand new notes");
    }

    [Fact]
    public async Task Handle_UpdateWithZeroUnitPrice_UpdatesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id);

        var command = new UpdateServiceItemCommand(
            serviceItem.Id,
            serviceRecord.Id,
            "Free Item",
            ServiceItemType.Part,
            0m,
            1,
            null,
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.UnitPrice.ShouldBe(0m);
        result.Value.TotalPrice.ShouldBe(0m);
    }
}
