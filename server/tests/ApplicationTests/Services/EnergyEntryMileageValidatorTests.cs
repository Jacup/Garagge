using Application.Services;
using Domain.Entities.EnergyEntries;

namespace ApplicationTests.Services;

public class EnergyEntryMileageValidatorTests
{
    private static EnergyEntry CreateEntry(Guid id, Guid vehicleId, DateOnly date, int mileage)
        => new()
        {
            Id = id,
            VehicleId = vehicleId,
            Vehicle = null!,
            Date = date,
            Mileage = mileage,
            Type = 0,
            EnergyUnit = 0,
            Volume = 1
        };

    [Fact]
    public void NoOtherEntries_ValidMileage_ReturnsTrue()
    {
        var vehicleId = Guid.NewGuid();
        var entry = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 1, 1), 1000);
        var entries = new List<EnergyEntry> { entry };
        var validator = new EnergyEntryMileageValidator();

        var result = validator.IsValid(entries, entry, entry.Date, entry.Mileage);

        result.ShouldBeTrue();
    }

    [Fact]
    public void LaterEntryWithLowerMileage_Invalid_ReturnsFalse()
    {
        var vehicleId = Guid.NewGuid();
        var entry = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 1, 1), 2000);
        var later = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 2, 1), 1500);
        var entries = new List<EnergyEntry> { entry, later };
        var validator = new EnergyEntryMileageValidator();

        var result = validator.IsValid(entries, entry, entry.Date, entry.Mileage);

        result.ShouldBeFalse();
    }

    [Fact]
    public void EarlierEntryWithHigherMileage_Invalid_ReturnsFalse()
    {
        var vehicleId = Guid.NewGuid();
        var entry = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 2, 1), 1500);
        var earlier = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 1, 1), 2000);
        var entries = new List<EnergyEntry> { entry, earlier };
        var validator = new EnergyEntryMileageValidator();

        var result = validator.IsValid(entries, entry, entry.Date, entry.Mileage);

        result.ShouldBeFalse();
    }

    [Fact]
    public void LaterEntryWithHigherMileage_Valid_ReturnsTrue()
    {
        var vehicleId = Guid.NewGuid();
        var entry = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 1, 1), 1000);
        var later = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 2, 1), 2000);
        var entries = new List<EnergyEntry> { entry, later };
        var validator = new EnergyEntryMileageValidator();

        var result = validator.IsValid(entries, entry, entry.Date, entry.Mileage);

        result.ShouldBeTrue();
    }

    [Fact]
    public void EarlierEntryWithLowerMileage_Valid_ReturnsTrue()
    {
        var vehicleId = Guid.NewGuid();
        var entry = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 2, 1), 2000);
        var earlier = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 1, 1), 1000);
        var entries = new List<EnergyEntry> { entry, earlier };
        var validator = new EnergyEntryMileageValidator();

        var result = validator.IsValid(entries, entry, entry.Date, entry.Mileage);

        result.ShouldBeTrue();
    }

    [Fact]
    public void LaterEntryWithEqualMileage_Valid_ReturnsTrue()
    {
        var vehicleId = Guid.NewGuid();
        var entry = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 1, 1), 1000);
        var later = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 2, 1), 1000);
        var entries = new List<EnergyEntry> { entry, later };
        var validator = new EnergyEntryMileageValidator();

        var result = validator.IsValid(entries, entry, entry.Date, entry.Mileage);

        result.ShouldBeTrue();
    }

    [Fact]
    public void EarlierEntryWithEqualMileage_Valid_ReturnsTrue()
    {
        var vehicleId = Guid.NewGuid();
        var entry = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 2, 1), 1000);
        var earlier = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 1, 1), 1000);
        var entries = new List<EnergyEntry> { entry, earlier };
        var validator = new EnergyEntryMileageValidator();

        var result = validator.IsValid(entries, entry, entry.Date, entry.Mileage);

        result.ShouldBeTrue();
    }

    [Fact]
    public void MultipleEntries_OneInvalidLater_ReturnsFalse()
    {
        var vehicleId = Guid.NewGuid();
        var entry = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 2, 1), 2000);
        var later1 = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 3, 1), 1500);
        var later2 = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 4, 1), 2500);
        var entries = new List<EnergyEntry> { entry, later1, later2 };
        var validator = new EnergyEntryMileageValidator();

        var result = validator.IsValid(entries, entry, entry.Date, entry.Mileage);

        result.ShouldBeFalse();
    }

    [Fact]
    public void MultipleEntries_OneInvalidEarlier_ReturnsFalse()
    {
        var vehicleId = Guid.NewGuid();
        var entry = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 2, 1), 1500);
        var earlier1 = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 1, 1), 2000);
        var earlier2 = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2023, 12, 1), 1000);
        var entries = new List<EnergyEntry> { entry, earlier1, earlier2 };
        var validator = new EnergyEntryMileageValidator();

        var result = validator.IsValid(entries, entry, entry.Date, entry.Mileage);

        result.ShouldBeFalse();
    }

    [Fact]
    public void EntriesForOtherVehicle_Ignored_ReturnsTrue()
    {
        var vehicleId = Guid.NewGuid();
        var otherVehicleId = Guid.NewGuid();
        var entry = CreateEntry(Guid.NewGuid(), vehicleId, new DateOnly(2024, 1, 1), 1000);
        var other = CreateEntry(Guid.NewGuid(), otherVehicleId, new DateOnly(2024, 2, 1), 500);
        var entries = new List<EnergyEntry> { entry, other };
        var validator = new EnergyEntryMileageValidator();

        var result = validator.IsValid(entries, entry, entry.Date, entry.Mileage);

        result.ShouldBeTrue();
    }
}