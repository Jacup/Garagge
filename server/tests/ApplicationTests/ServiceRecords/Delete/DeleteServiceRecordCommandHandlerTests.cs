using Application.ServiceRecords;
using Application.ServiceRecords.Delete;
using Domain.Entities.Services;
using Domain.Enums;
using Domain.Enums.Services;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.ServiceRecords.Delete;

public class DeleteServiceRecordCommandHandlerTests : InMemoryDbTestBase
{
    private readonly DeleteServiceRecordCommandHandler _handler;

    public DeleteServiceRecordCommandHandlerTests()
    {
        _handler = new DeleteServiceRecordCommandHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidServiceRecordDeletion_ReturnsSuccess()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        
        var command = new DeleteServiceRecordCommand(serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var deletedRecord = await Context.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ServiceRecordNotFound_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var nonExistentRecordId = Guid.NewGuid();
        
        var command = new DeleteServiceRecordCommand(nonExistentRecordId, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound(nonExistentRecordId));
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        var nonExistentVehicleId = Guid.NewGuid();
        
        var command = new DeleteServiceRecordCommand(serviceRecord.Id, nonExistentVehicleId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound(serviceRecord.Id));
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
        
        var command = new DeleteServiceRecordCommand(serviceRecord.Id, vehicle2.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.NotFound(serviceRecord.Id));
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsFailureWithUnauthorizedError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline], otherUserId);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        
        var command = new DeleteServiceRecordCommand(serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_ServiceRecordWithItems_DeletesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        await CreateServiceItemInDb(serviceRecord.Id, "Oil Filter");
        await CreateServiceItemInDb(serviceRecord.Id, "Engine Oil");
        
        var command = new DeleteServiceRecordCommand(serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var deletedRecord = await Context.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();

        // Verify cascade delete of items
        var items = await Context.ServiceItems.Where(si => si.ServiceRecordId == serviceRecord.Id).ToListAsync();
        items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_ServiceRecordWithoutItems_DeletesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id);
        
        var command = new DeleteServiceRecordCommand(serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var deletedRecord = await Context.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_MultipleServiceRecords_DeletesOnlySpecifiedOne()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");
        var serviceRecord1 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Record 1");
        var serviceRecord2 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Record 2");
        var serviceRecord3 = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, "Record 3");
        
        var command = new DeleteServiceRecordCommand(serviceRecord2.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var deletedRecord = await Context.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord2.Id);
        deletedRecord.ShouldBeNull();

        var remainingRecords = await Context.ServiceRecords
            .Where(sr => sr.VehicleId == vehicle.Id)
            .ToListAsync();
        remainingRecords.Count.ShouldBe(2);
        remainingRecords.ShouldContain(sr => sr.Id == serviceRecord1.Id);
        remainingRecords.ShouldContain(sr => sr.Id == serviceRecord3.Id);
    }

    [Fact]
    public async Task Handle_ServiceRecordWithMileage_DeletesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Inspection");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, mileage: 50000);
        
        var command = new DeleteServiceRecordCommand(serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var deletedRecord = await Context.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ServiceRecordWithManualCost_DeletesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Repair");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, manualCost: 250.50m);
        
        var command = new DeleteServiceRecordCommand(serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var deletedRecord = await Context.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ServiceRecordWithNotes_DeletesSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Custom");
        var serviceRecord = await CreateServiceRecordInDb(vehicle.Id, serviceType.Id, notes: "Important notes about this service");
        
        var command = new DeleteServiceRecordCommand(serviceRecord.Id, vehicle.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var deletedRecord = await Context.ServiceRecords.FirstOrDefaultAsync(sr => sr.Id == serviceRecord.Id);
        deletedRecord.ShouldBeNull();
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
        int? mileage = null,
        decimal? manualCost = null,
        string? notes = null)
    {
        var serviceRecord = new ServiceRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            TypeId = serviceTypeId,
            Title = title,
            ServiceDate = DateTime.UtcNow.Date,
            Notes = notes,
            Mileage = mileage ?? 15000,
            ManualCost = manualCost
        };

        Context.ServiceRecords.Add(serviceRecord);
        await Context.SaveChangesAsync();
        return serviceRecord;
    }

    private async Task CreateServiceItemInDb(Guid serviceRecordId, string name = "Test Item")
    {
        var serviceItem = new ServiceItem
        {
            Id = Guid.NewGuid(),
            ServiceRecordId = serviceRecordId,
            Name = name,
            Type = ServiceItemType.Part,
            UnitPrice = 100.00m,
            Quantity = 1,
            PartNumber = null,
            Notes = null
        };

        Context.ServiceItems.Add(serviceItem);
        await Context.SaveChangesAsync();
    }
}