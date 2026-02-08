using Application.ServiceItems;
using Application.ServiceItems.Delete;
using Domain.Enums;
using Domain.Enums.Services;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.ServiceItems.Delete;

public class DeleteServiceItemCommandHandlerTests : InMemoryDbTestBase
{
    private readonly DeleteServiceItemCommandHandler _handler;

    public DeleteServiceItemCommandHandlerTests()
    {
        _handler = new DeleteServiceItemCommandHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidServiceItemDeletion_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id);
        
        var command = new DeleteServiceItemCommand(serviceItem.Id, serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var deletedItem = await Context.ServiceItems.FirstOrDefaultAsync(si => si.Id == serviceItem.Id);
        deletedItem.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ServiceItemNotFound_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var nonExistentItemId = Guid.NewGuid();
        
        var command = new DeleteServiceItemCommand(nonExistentItemId, serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceItemsErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ServiceRecordNotFound_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id);
        var nonExistentRecordId = Guid.NewGuid();
        
        var command = new DeleteServiceItemCommand(serviceItem.Id, nonExistentRecordId, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceItemsErrors.NotFound);
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id);
        var nonExistentVehicleId = Guid.NewGuid();
        
        var command = new DeleteServiceItemCommand(serviceItem.Id, serviceRecord.Id, nonExistentVehicleId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceItemsErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ServiceItemBelongsToDifferentServiceRecord_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord1 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceRecord2 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord1.Id);
        
        var command = new DeleteServiceItemCommand(serviceItem.Id, serviceRecord2.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceItemsErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ServiceRecordBelongsToDifferentVehicle_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle1 = await CreateVehicleInDb([EnergyType.Gasoline]);
        var vehicle2 = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle1.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id);
        
        var command = new DeleteServiceItemCommand(serviceItem.Id, serviceRecord.Id, vehicle2.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceItemsErrors.NotFound);
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline], otherUserId);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id);
        
        var command = new DeleteServiceItemCommand(serviceItem.Id, serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceItemsErrors.NotFound);
    }

    [Fact]
    public async Task Handle_MultipleServiceItems_DeletesOnlySpecifiedOne()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem1 = await CreateServiceItemInDb(serviceRecord.Id, "Item 1");
        var serviceItem2 = await CreateServiceItemInDb(serviceRecord.Id, "Item 2");
        var serviceItem3 = await CreateServiceItemInDb(serviceRecord.Id, "Item 3");
        
        var command = new DeleteServiceItemCommand(serviceItem2.Id, serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var deletedItem = await Context.ServiceItems.FirstOrDefaultAsync(si => si.Id == serviceItem2.Id);
        deletedItem.ShouldBeNull();

        var remainingItems = await Context.ServiceItems
            .Where(si => si.ServiceRecordId == serviceRecord.Id)
            .ToListAsync();
        remainingItems.Count.ShouldBe(2);
        remainingItems.ShouldContain(si => si.Id == serviceItem1.Id);
        remainingItems.ShouldContain(si => si.Id == serviceItem3.Id);
    }

    [Fact]
    public async Task Handle_PartTypeServiceItem_DeletesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Repair");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id, type: ServiceItemType.Part);
        
        var command = new DeleteServiceItemCommand(serviceItem.Id, serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var deletedItem = await Context.ServiceItems.FirstOrDefaultAsync(si => si.Id == serviceItem.Id);
        deletedItem.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_LaborTypeServiceItem_DeletesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Repair");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id, type: ServiceItemType.Labor);
        
        var command = new DeleteServiceItemCommand(serviceItem.Id, serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var deletedItem = await Context.ServiceItems.FirstOrDefaultAsync(si => si.Id == serviceItem.Id);
        deletedItem.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ServiceItemWithPartNumber_DeletesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Repair");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id, partNumber: "ABC123");
        
        var command = new DeleteServiceItemCommand(serviceItem.Id, serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var deletedItem = await Context.ServiceItems.FirstOrDefaultAsync(si => si.Id == serviceItem.Id);
        deletedItem.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ServiceItemWithNotes_DeletesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Repair");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var serviceItem = await CreateServiceItemInDb(serviceRecord.Id, notes: "Some important notes");
        
        var command = new DeleteServiceItemCommand(serviceItem.Id, serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var deletedItem = await Context.ServiceItems.FirstOrDefaultAsync(si => si.Id == serviceItem.Id);
        deletedItem.ShouldBeNull();
    }
}
