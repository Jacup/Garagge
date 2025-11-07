using Application.ServiceRecords;
using Application.ServiceRecords.Update;
using Domain.Entities.Services;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.ServiceRecords.Update;

public class UpdateServiceRecordCommandHandlerTests : InMemoryDbTestBase
{
    private readonly UpdateServiceRecordCommandHandler _handler;

    public UpdateServiceRecordCommandHandlerTests()
    {
        _handler = new UpdateServiceRecordCommandHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithUpdatedServiceRecordDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var newServiceType = await CreateServiceTypeInDb("Inspection");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new UpdateServiceRecordCommand(
            serviceRecord.Id,
            "Updated Oil Change",
            "Updated notes",
            20000,
            new DateTime(2024, 11, 1),
            200.00m,
            newServiceType.Id,
            vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(serviceRecord.Id);
        result.Value.Title.ShouldBe("Updated Oil Change");
        result.Value.Notes.ShouldBe("Updated notes");
        result.Value.Mileage.ShouldBe(20000);
        result.Value.ServiceDate.ShouldBe(new DateTime(2024, 11, 1));
        result.Value.TotalCost.ShouldBe(200.00m);
        result.Value.TypeId.ShouldBe(newServiceType.Id);
        result.Value.Type.ShouldBe("Inspection");
        result.Value.VehicleId.ShouldBe(vehicle.Id);

        var updatedEntity = await Context.ServiceRecords
            .Include(sr => sr.Type)
            .FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);

        updatedEntity.ShouldNotBeNull();
        updatedEntity.Title.ShouldBe("Updated Oil Change");
        updatedEntity.Notes.ShouldBe("Updated notes");
        updatedEntity.Mileage.ShouldBe(20000);
        updatedEntity.ServiceDate.ShouldBe(new DateTime(2024, 11, 1));
        updatedEntity.ManualCost.ShouldBe(200.00m);
        updatedEntity.TypeId.ShouldBe(newServiceType.Id);
        updatedEntity.Type!.Name.ShouldBe("Inspection");
    }

    [Fact]
    public async Task Handle_ValidCommandWithNullValues_ReturnsSuccessWithUpdatedServiceRecordDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(
            vehicle.Id,
            serviceType.Id,
            "Old Title",
            "Old notes",
            15000,
            100.00m);

        var command = new UpdateServiceRecordCommand(
            serviceRecord.Id,
            "Inspection",
            null,
            null,
            new DateTime(2024, 11, 1),
            null,
            serviceType.Id,
            vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Title.ShouldBe("Inspection");
        result.Value.Notes.ShouldBeNull();
        result.Value.Mileage.ShouldBeNull();
        result.Value.TotalCost.ShouldBe(0m);

        var updatedEntity = await Context.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);

        updatedEntity.ShouldNotBeNull();
        updatedEntity.Notes.ShouldBeNull();
        updatedEntity.Mileage.ShouldBeNull();
        updatedEntity.ManualCost.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ServiceRecordNotFound_ReturnsServiceRecordNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var nonExistentServiceRecordId = Guid.NewGuid();

        var command = new UpdateServiceRecordCommand(
            nonExistentServiceRecordId,
            "Test Title",
            null,
            null,
            new DateTime(2024, 11, 1),
            null,
            serviceType.Id,
            vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound(nonExistentServiceRecordId));
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsVehicleNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var nonExistentVehicleId = Guid.NewGuid();

        var command = new UpdateServiceRecordCommand(
            serviceRecord.Id,
            "Test Title",
            null,
            null,
            new DateTime(2024, 11, 1),
            null,
            serviceType.Id,
            nonExistentVehicleId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound(serviceRecord.Id));
    }

    [Fact]
    public async Task Handle_ServiceRecordBelongsToDifferentVehicle_ReturnsServiceRecordNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle1 = await CreateVehicleInDb([EnergyType.Gasoline]);
        var vehicle2 = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle1.Id, serviceType.Id);

        var command = new UpdateServiceRecordCommand(
            serviceRecord.Id,
            "Test Title",
            null,
            null,
            new DateTime(2024, 11, 1),
            null,
            serviceType.Id,
            vehicle2.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound(serviceRecord.Id));
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

        var command = new UpdateServiceRecordCommand(
            serviceRecord.Id,
            "Test Title",
            null,
            null,
            new DateTime(2024, 11, 1),
            null,
            serviceType.Id,
            vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_ServiceTypeNotFound_ReturnsServiceTypeNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var nonExistentServiceTypeId = Guid.NewGuid();

        var command = new UpdateServiceRecordCommand(
            serviceRecord.Id,
            "Test Title",
            null,
            null,
            new DateTime(2024, 11, 1),
            null,
            nonExistentServiceTypeId,
            vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.ServiceTypeNotFound(nonExistentServiceTypeId));
    }

    [Fact]
    public async Task Handle_UpdateOnlyTitle_KeepsOtherFieldsIntact()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(
            vehicle.Id,
            serviceType.Id,
            "Original Title",
            "Original notes",
            15000,
            100.00m);

        var command = new UpdateServiceRecordCommand(
            serviceRecord.Id,
            "Updated Title Only",
            "Original notes",
            15000,
            serviceRecord.ServiceDate,
            100.00m,
            serviceType.Id,
            vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Title.ShouldBe("Updated Title Only");
        result.Value.Notes.ShouldBe("Original notes");
        result.Value.Mileage.ShouldBe(15000);
    }

    [Fact]
    public async Task Handle_UpdateAllFields_UpdatesAllFieldsCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var oldServiceType = await CreateServiceTypeInDb("Oil Change");
        var newServiceType = await CreateServiceTypeInDb("Major Service");
        var serviceRecord = await CreateServiceRecordInDb(
            vehicle.Id,
            oldServiceType.Id,
            "Old Title",
            "Old notes",
            10000,
            50.00m);

        var command = new UpdateServiceRecordCommand(
            serviceRecord.Id,
            "Completely New Title",
            "Brand new notes",
            25000,
            new DateTime(2024, 10, 15),
            500.00m,
            newServiceType.Id,
            vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var updatedEntity = await Context.ServiceRecords
            .Include(sr => sr.Type)
            .FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);

        updatedEntity.ShouldNotBeNull();
        updatedEntity.Title.ShouldBe("Completely New Title");
        updatedEntity.Notes.ShouldBe("Brand new notes");
        updatedEntity.Mileage.ShouldBe(25000);
        updatedEntity.ServiceDate.ShouldBe(new DateTime(2024, 10, 15));
        updatedEntity.ManualCost.ShouldBe(500.00m);
        updatedEntity.TotalCost.ShouldBe(500.00m);
        updatedEntity.TypeId.ShouldBe(newServiceType.Id);
        updatedEntity.Type!.Name.ShouldBe("Major Service");
    }

    [Fact]
    public async Task Handle_UpdateDoesNotAffectServiceItems_KeepsServiceItemsIntact()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        await CreateServiceItemInDb(serviceRecord.Id, "Item 1");
        await CreateServiceItemInDb(serviceRecord.Id, "Item 2");

        var command = new UpdateServiceRecordCommand(
            serviceRecord.Id,
            "Updated Title",
            "Updated notes",
            20000,
            new DateTime(2024, 11, 1),
            null,
            serviceType.Id,
            vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ServiceItems.Count.ShouldBe(2);
        result.Value.ServiceItems.Any(i => i.Name == "Item 1").ShouldBeTrue();
        result.Value.ServiceItems.Any(i => i.Name == "Item 2").ShouldBeTrue();

        var items = await Context.ServiceItems
            .Where(si => si.ServiceRecordId == serviceRecord.Id)
            .ToListAsync();

        items.Count.ShouldBe(2);
        items.Any(i => i.Name == "Item 1").ShouldBeTrue();
        items.Any(i => i.Name == "Item 2").ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_UpdateWithZeroManualCost_UpdatesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);

        var command = new UpdateServiceRecordCommand(
            serviceRecord.Id,
            "Free Service",
            null,
            null,
            new DateTime(2024, 11, 1),
            0m,
            serviceType.Id,
            vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(0m);
    }

    private async Task<ServiceType> CreateServiceTypeInDb(string name)
    {
        var serviceType = new ServiceType
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        Context.ServiceTypes.Add(serviceType);
        await Context.SaveChangesAsync();
        return serviceType;
    }

    private async Task<ServiceRecord> CreateServiceRecordInDb(
        Guid vehicleId,
        Guid serviceTypeId,
        string title = "Test Service Record",
        string? notes = null,
        int? mileage = 15000,
        decimal? manualCost = null)
    {
        var serviceRecord = new ServiceRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            TypeId = serviceTypeId,
            Title = title,
            ServiceDate = DateTime.UtcNow.Date,
            Notes = notes,
            Mileage = mileage,
            ManualCost = manualCost
        };

        Context.ServiceRecords.Add(serviceRecord);
        await Context.SaveChangesAsync();
        return serviceRecord;
    }

    private async Task CreateServiceItemInDb(Guid serviceRecordId,
        string name = "Test Item")
    {
        var serviceItem = new ServiceItem
        {
            Id = Guid.NewGuid(),
            ServiceRecordId = serviceRecordId,
            Name = name,
            Type = Domain.Enums.Services.ServiceItemType.Part,
            UnitPrice = 100.00m,
            Quantity = 1,
            PartNumber = null,
            Notes = null
        };

        Context.ServiceItems.Add(serviceItem);
        await Context.SaveChangesAsync();
    }
}

