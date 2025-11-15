using Application.ServiceItems.Create;
using Application.ServiceRecords;
using Application.ServiceRecords.Create;
using Application.Vehicles;
using Domain.Enums;
using Domain.Enums.Services;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.ServiceRecords.Create;

public class CreateServiceRecordCommandHandlerTests : InMemoryDbTestBase
{
    private readonly CreateServiceRecordsCommandHandler _handler;

    public CreateServiceRecordCommandHandlerTests()
    {
        _handler = new CreateServiceRecordsCommandHandler(Context, UserContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithServiceRecordDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");

        var command = new CreateServiceRecordCommand(
            "Regular oil change",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "Used synthetic oil",
            15000,
            150.50m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Title.ShouldBe("Regular oil change");
        result.Value.Notes.ShouldBe("Used synthetic oil");
        result.Value.Mileage.ShouldBe(15000);
        result.Value.ServiceDate.ShouldBe(new DateTime(2024, 11, 5));
        result.Value.TotalCost.ShouldBe(150.50m);
        result.Value.TypeId.ShouldBe(serviceType.Id);
        result.Value.Type.ShouldNotBeNull();
        result.Value.Type.ShouldBe("Oil Change");

        var addedEntity = Context.ServiceRecords.SingleOrDefault(sr => sr.Id == result.Value.Id);

        addedEntity.ShouldNotBeNull();
        addedEntity.Title.ShouldBe("Regular oil change");
        addedEntity.Notes.ShouldBe("Used synthetic oil");
        addedEntity.Mileage.ShouldBe(15000);
        addedEntity.ServiceDate.ShouldBe(new DateTime(2024, 11, 5));
        addedEntity.ManualCost.ShouldBe(150.50m);
        addedEntity.VehicleId.ShouldBe(vehicle.Id);
        addedEntity.TypeId.ShouldBe(serviceType.Id);
    }

    [Fact]
    public async Task Handle_ValidCommandWithMinimalData_ReturnsSuccessWithServiceRecordDto()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Inspection");

        var command = new CreateServiceRecordCommand(
            "Basic inspection",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            null,
            null,
            null,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Title.ShouldBe("Basic inspection");
        result.Value.Notes.ShouldBeNull();
        result.Value.Mileage.ShouldBeNull();
        result.Value.TotalCost.ShouldBe(0m);
        result.Value.ServiceDate.ShouldBe(new DateTime(2024, 11, 5));
    }

    [Fact]
    public async Task Handle_VehicleNotFound_ReturnsVehicleNotFoundError()
    {
        // Arrange
        SetupAuthorizedUser();
        var nonExistentVehicleId = Guid.NewGuid();
        var serviceType = await CreateServiceTypeInDb("Oil Change");

        var command = new CreateServiceRecordCommand(
            "Regular oil change",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            nonExistentVehicleId,
            null,
            15000,
            150.50m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(VehicleErrors.NotFound(nonExistentVehicleId));
    }

    [Fact]
    public async Task Handle_VehicleNotOwnedByUser_ReturnsUnauthorizedError()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline], otherUserId);
        var serviceType = await CreateServiceTypeInDb("Oil Change");

        var command = new CreateServiceRecordCommand(
            "Regular oil change",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            null,
            15000,
            150.50m,
            new List<CreateServiceItemCommand>());

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
        var nonExistentServiceTypeId = Guid.NewGuid();

        var command = new CreateServiceRecordCommand(
            "Regular oil change",
            new DateTime(2024, 11, 5),
            nonExistentServiceTypeId,
            vehicle.Id,
            null,
            15000,
            150.50m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.ServiceTypeNotFound(nonExistentServiceTypeId));
    }

    [Fact]
    public async Task Handle_WhenUserIdIsEmpty_ReturnsUnauthorizedError()
    {
        // Arrange
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");

        var command = new CreateServiceRecordCommand(
            "Regular oil change",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            null,
            15000,
            150.50m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ServiceRecordErrors.Unauthorized);
    }

    [Fact]
    public async Task Handle_ValidCommand_GeneratesNewGuidForServiceRecordId()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");

        var command = new CreateServiceRecordCommand(
            "Regular oil change",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            null,
            15000,
            150.50m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithHighMileage_CreatesServiceRecordSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Major Service");

        var command = new CreateServiceRecordCommand(
            "Major Service at 500K",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "High mileage service",
            500000,
            1500.00m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Mileage.ShouldBe(500000);
        result.Value.TotalCost.ShouldBe(1500.00m);
    }

    [Fact]
    public async Task Handle_WithLargeCost_CreatesServiceRecordSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Engine Replacement");

        var command = new CreateServiceRecordCommand(
            "Engine replacement",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "Full engine swap",
            150000,
            15000.99m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(15000.99m);
    }

    [Fact]
    public async Task Handle_WithLongNotes_CreatesServiceRecordSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Maintenance");

        var longNotes = new string('A', 1000);
        var command = new CreateServiceRecordCommand(
            "Detailed maintenance",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            longNotes,
            15000,
            150.50m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Notes.ShouldBe(longNotes);
    }
    
    [Fact]
    public async Task Handle_WithPastDate_CreatesServiceRecordSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Historical Service");

        var pastDate = new DateTime(2020, 1, 1);
        var command = new CreateServiceRecordCommand(
            "Historical maintenance",
            pastDate,
            serviceType.Id,
            vehicle.Id,
            "Service from 2020",
            10000,
            100.00m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ServiceDate.ShouldBe(pastDate);
    }

    [Fact]
    public async Task Handle_MultipleServiceRecordsForSameVehicle_CreatesAllSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");

        var command1 = new CreateServiceRecordCommand(
            "First oil change",
            new DateTime(2024, 1, 1),
            serviceType.Id,
            vehicle.Id,
            null,
            10000,
            100.00m,
            new List<CreateServiceItemCommand>());

        var command2 = new CreateServiceRecordCommand(
            "Second oil change",
            new DateTime(2024, 6, 1),
            serviceType.Id,
            vehicle.Id,
            null,
            20000,
            100.00m,
            new List<CreateServiceItemCommand>());

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();
        result1.Value.Id.ShouldNotBe(result2.Value.Id);

        var recordsInDb = Context.ServiceRecords.Where(sr => sr.VehicleId == vehicle.Id).ToList();
        recordsInDb.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WithZeroMileage_CreatesServiceRecordSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Initial Setup");

        var command = new CreateServiceRecordCommand(
            "Initial setup",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "Brand new car",
            0,
            50.00m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Mileage.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WithZeroCost_CreatesServiceRecordSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Warranty Service");

        var command = new CreateServiceRecordCommand(
            "Warranty repair",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "Covered under warranty",
            15000,
            0.00m,
            new List<CreateServiceItemCommand>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(0.00m);
    }

    [Fact]
    public async Task Handle_WithServiceItems_CreatesServiceRecordWithItems()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Oil Change");

        var serviceItems = new List<CreateServiceItemCommand>
        {
            new(
                Guid.Empty, // Will be set by handler
                "Engine Oil 5W-30",
                ServiceItemType.Part,
                45.00m,
                5,
                "12345",
                "Premium synthetic oil"),
            new(
                Guid.Empty,
                "Oil Filter",
                ServiceItemType.Part,
                15.00m,
                1,
                "67890",
                null),
            new(
                Guid.Empty,
                "Labor",
                ServiceItemType.Labor,
                80.00m,
                1,
                null,
                "Oil change service")
        };

        var command = new CreateServiceRecordCommand(
            "Complete oil change",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "Full service with premium oil",
            15000,
            null,
            serviceItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ServiceItems.ShouldNotBeNull();
        result.Value.ServiceItems.Count.ShouldBe(3);

        // Verify total cost calculation (45*5 + 15*1 + 80*1 = 320)
        result.Value.TotalCost.ShouldBe(320.00m);

        // Verify items in database
        var recordInDb = await Context.ServiceRecords
            .Include(sr => sr.Items)
            .FirstOrDefaultAsync(sr => sr.Id == result.Value.Id);

        recordInDb.ShouldNotBeNull();
        recordInDb.Items.Count.ShouldBe(3);

        var oilItem = recordInDb.Items.FirstOrDefault(i => i.Name == "Engine Oil 5W-30");
        oilItem.ShouldNotBeNull();
        oilItem.Type.ShouldBe(ServiceItemType.Part);
        oilItem.UnitPrice.ShouldBe(45.00m);
        oilItem.Quantity.ShouldBe(5);
        oilItem.TotalPrice.ShouldBe(225.00m);
        oilItem.PartNumber.ShouldBe("12345");
        oilItem.Notes.ShouldBe("Premium synthetic oil");
    }

    [Fact]
    public async Task Handle_WithSingleServiceItem_CalculatesTotalCostCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Tire Rotation");

        var serviceItems = new List<CreateServiceItemCommand>
        {
            new(
                Guid.Empty,
                "Tire rotation service",
                ServiceItemType.Labor,
                50.00m,
                1,
                null,
                null)
        };

        var command = new CreateServiceRecordCommand(
            "Tire rotation",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            null,
            20000,
            null,
            serviceItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(50.00m);
        result.Value.ServiceItems.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WithServiceItemsAndManualCost_UsesServiceItemsCostOnly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Brake Service");

        var serviceItems = new List<CreateServiceItemCommand>
        {
            new(
                Guid.Empty,
                "Brake pads",
                ServiceItemType.Part,
                100.00m,
                2,
                "BRAKE123",
                null),
            new(
                Guid.Empty,
                "Installation",
                ServiceItemType.Labor,
                75.00m,
                1,
                null,
                null)
        };

        var command = new CreateServiceRecordCommand(
            "Brake replacement",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "Front brake pads",
            25000,
            500.00m, // This should be ignored when Items exist
            serviceItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // TotalCost should be from items (100*2 + 75*1 = 275), not ManualCost (500)
        result.Value.TotalCost.ShouldBe(275.00m);
        result.Value.ServiceItems.Count.ShouldBe(2);

        var recordInDb = await Context.ServiceRecords
            .Include(sr => sr.Items)
            .FirstOrDefaultAsync(sr => sr.Id == result.Value.Id);

        recordInDb.ShouldNotBeNull();
        recordInDb.ManualCost.ShouldBe(500.00m); // ManualCost is stored
        recordInDb.TotalCost.ShouldBe(275.00m); // But TotalCost comes from Items
    }

    [Fact]
    public async Task Handle_WithMultipleServiceItemTypes_CreatesAllTypesCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Major Service");

        var serviceItems = new List<CreateServiceItemCommand>
        {
            new(
                Guid.Empty,
                "Air filter",
                ServiceItemType.Part,
                25.00m,
                1,
                "AF001",
                null),
            new(
                Guid.Empty,
                "Cabin filter",
                ServiceItemType.Part,
                30.00m,
                1,
                "CF001",
                null),
            new(
                Guid.Empty,
                "Diagnostic",
                ServiceItemType.Labor,
                60.00m,
                1,
                null,
                null),
            new(
                Guid.Empty,
                "Installation labor",
                ServiceItemType.Labor,
                90.00m,
                1.5m,
                null,
                "1.5 hours"),
            new(
                Guid.Empty,
                "Sales tax",
                ServiceItemType.Tax,
                5.50m,
                1,
                null,
                "8% tax"),
            new(
                Guid.Empty,
                "Shop supplies",
                ServiceItemType.Other,
                10.00m,
                1,
                null,
                null)
        };

        var command = new CreateServiceRecordCommand(
            "Comprehensive service",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "Complete checkup with filters",
            30000,
            null,
            serviceItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ServiceItems.Count.ShouldBe(6);

        // Verify total: 25 + 30 + 60 + 90*1.5 + 5.50 + 10 = 265.50
        result.Value.TotalCost.ShouldBe(265.50m);

        var recordInDb = await Context.ServiceRecords
            .Include(sr => sr.Items)
            .FirstOrDefaultAsync(sr => sr.Id == result.Value.Id);

        recordInDb.ShouldNotBeNull();
        recordInDb.Items.Count(i => i.Type == ServiceItemType.Part).ShouldBe(2);
        recordInDb.Items.Count(i => i.Type == ServiceItemType.Labor).ShouldBe(2);
        recordInDb.Items.Count(i => i.Type == ServiceItemType.Tax).ShouldBe(1);
        recordInDb.Items.Count(i => i.Type == ServiceItemType.Other).ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WithServiceItemsWithDecimalQuantity_CalculatesCorrectly()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Custom Work");

        var serviceItems = new List<CreateServiceItemCommand>
        {
            new(
                Guid.Empty,
                "Specialized labor",
                ServiceItemType.Labor,
                100.00m,
                2.5m,
                null,
                "2.5 hours of work")
        };

        var command = new CreateServiceRecordCommand(
            "Custom repair",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            null,
            15000,
            null,
            serviceItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(250.00m); // 100 * 2.5

        var item = result.Value.ServiceItems.First();
        item.Quantity.ShouldBe(2.5m);
        item.TotalPrice.ShouldBe(250.00m);
    }

    [Fact]
    public async Task Handle_WithServiceItemsWithZeroQuantity_CalculatesTotalAsZero()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Quote");

        var serviceItems = new List<CreateServiceItemCommand>
        {
            new(
                Guid.Empty,
                "Future part",
                ServiceItemType.Part,
                100.00m,
                0,
                "PART123",
                "For quote purposes only")
        };

        var command = new CreateServiceRecordCommand(
            "Service quote",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "Just a quote, no work done yet",
            15000,
            null,
            serviceItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(0.00m);
        result.Value.ServiceItems.First().TotalPrice.ShouldBe(0.00m);
    }

    [Fact]
    public async Task Handle_WithEmptyServiceItemsList_UsesManualCost()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Quick Service");

        var command = new CreateServiceRecordCommand(
            "Quick fix",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "Minor adjustment",
            15000,
            25.00m,
            new List<CreateServiceItemCommand>()); // Empty list

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCost.ShouldBe(25.00m); // Should use ManualCost
        result.Value.ServiceItems.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WithLargeNumberOfServiceItems_CreatesAllSuccessfully()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = await CreateVehicleInDb([EnergyType.Gasoline]);
        var serviceType = await CreateServiceTypeInDb("Complex Repair");

        var serviceItems = new List<CreateServiceItemCommand>();
        for (int i = 1; i <= 20; i++)
        {
            serviceItems.Add(new CreateServiceItemCommand(
                Guid.Empty,
                $"Part {i}",
                ServiceItemType.Part,
                10.00m,
                1,
                $"PART{i:D3}",
                null));
        }

        var command = new CreateServiceRecordCommand(
            "Complex repair with many parts",
            new DateTime(2024, 11, 5),
            serviceType.Id,
            vehicle.Id,
            "Multiple component replacement",
            50000,
            null,
            serviceItems);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ServiceItems.Count.ShouldBe(20);
        result.Value.TotalCost.ShouldBe(200.00m); // 20 items * 10 * 1

        var recordInDb = await Context.ServiceRecords
            .Include(sr => sr.Items)
            .FirstOrDefaultAsync(sr => sr.Id == result.Value.Id);

        recordInDb.ShouldNotBeNull();
        recordInDb.Items.Count.ShouldBe(20);
    }
}

