using Application.Abstractions;
using Application.Services;
using Domain.Entities.EnergyEntries;
using Domain.Entities.Services;
using Domain.Entities.Vehicles;
using Domain.Enums;
using Domain.Enums.Services;
using Moq;

namespace ApplicationTests.Services;

public class StatisticsServiceTests : InMemoryDbTestBase
{
    private readonly StatisticsService _sut;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;

    public StatisticsServiceTests()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 29));
        _sut = new StatisticsService(Context, _dateTimeProviderMock.Object);
    }

    #region GetVehicleScope Tests

    [Fact]
    public void GetVehicleScope_UserRole_ReturnsOnlyAuthorizedUserVehicles()
    {
        // Arrange
        SetupAuthorizedUser();
        var authorizedUserVehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };
        var otherUserVehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "BMW",
            Model = "3 Series",
            EngineType = EngineType.Electric,
            UserId = Guid.NewGuid()
        };

        Context.Vehicles.AddRange(authorizedUserVehicle, otherUserVehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "User").ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(authorizedUserVehicle.Id);
        result[0].UserId.ShouldBe(AuthorizedUserId);
    }

    [Fact]
    public void GetVehicleScope_UserRoleWithNoVehicles_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserVehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Audi",
            Model = "A4",
            EngineType = EngineType.Fuel,
            UserId = Guid.NewGuid()
        };

        Context.Vehicles.Add(otherUserVehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "User").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetVehicleScope_UserRoleMultipleVehicles_ReturnsAllAuthorizedUserVehicles()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicles = new[]
        {
            new Vehicle
            {
                Id = Guid.NewGuid(),
                Brand = "Toyota",
                Model = "Corolla",
                EngineType = EngineType.Fuel,
                UserId = AuthorizedUserId
            },
            new Vehicle
            {
                Id = Guid.NewGuid(),
                Brand = "Honda",
                Model = "Civic",
                EngineType = EngineType.Fuel,
                UserId = AuthorizedUserId
            },
            new Vehicle
            {
                Id = Guid.NewGuid(),
                Brand = "Tesla",
                Model = "Model 3",
                EngineType = EngineType.Electric,
                UserId = AuthorizedUserId
            }
        };

        Context.Vehicles.AddRange(vehicles);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "User").ToList();

        // Assert
        result.Count.ShouldBe(3);
        result.Select(v => v.Id).ShouldBe(vehicles.Select(v => v.Id));
    }

    [Fact]
    public void GetVehicleScope_UnknownRole_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "UnknownRole").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetVehicleScope_AdminRole_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "Admin").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetVehicleScope_UserRole_CanIterateMultipleTimes()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        // Act
        var queryable = _sut.GetVehicleScope(AuthorizedUserId, "User");
        var firstIteration = queryable.ToList();
        var secondIteration = queryable.ToList();

        // Assert
        firstIteration.Count.ShouldBe(1);
        secondIteration.Count.ShouldBe(1);
        firstIteration[0].Id.ShouldBe(secondIteration[0].Id);
    }

    [Fact]
    public void GetVehicleScope_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizedUser();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "User").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetVehicleScope_NullRole_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizedUser();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = AuthorizedUserId
        };

        Context.Vehicles.Add(vehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, null!).ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void GetVehicleScope_DifferentUserId_ReturnsEmpty()
    {
        // Arrange
        SetupAuthorizedUser();
        var otherUserId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            EngineType = EngineType.Fuel,
            UserId = otherUserId
        };

        Context.Vehicles.Add(vehicle);
        Context.SaveChanges();

        // Act
        var result = _sut.GetVehicleScope(AuthorizedUserId, "User").ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    #endregion

    #region CalculateMileageStats Tests

    [Fact]
    public void CalculateMileageStats_WithValidEnergyAndServiceEntries_ReturnsCorrectMileageCalculations()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 50,
                Date = new DateOnly(2024, 2, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(1000);
        result.LastMileage.ShouldBe(3000);
        result.DistanceDriven.ShouldBe(2000);
    }

    [Fact]
    public void CalculateMileageStats_WithValidServiceEntries_ReturnsCorrectMileageCalculations()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service2",
                Mileage = 3500,
                ServiceDate = new DateTime(2024, 2, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(1500);
        result.LastMileage.ShouldBe(3500);
        result.DistanceDriven.ShouldBe(2000);
    }

    [Fact]
    public void CalculateMileageStats_WithMixedEnergyAndServiceEntries_ReturnsCombinedMaxMinMileage()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 500,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2500,
                Volume = 50,
                Date = new DateOnly(2024, 2, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1000,
                ServiceDate = new DateTime(2024, 1, 15),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service2",
                Mileage = 3000,
                ServiceDate = new DateTime(2024, 2, 15),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(500);
        result.LastMileage.ShouldBe(3000);
        result.DistanceDriven.ShouldBe(2500);
    }

    [Fact]
    public void CalculateMileageStats_EmptyEnergyAndServiceEntries_ReturnsZeroMileage()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(0);
        result.LastMileage.ShouldBe(0);
        result.DistanceDriven.ShouldBe(0);
    }

    [Fact]
    public void CalculateMileageStats_OnlyEnergyEntriesEmpty_ReturnsZeroMileage()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = null,
                ServiceDate = new DateTime(2024, 1, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(0);
        result.LastMileage.ShouldBe(0);
        result.DistanceDriven.ShouldBe(0);
    }

    [Fact]
    public void CalculateMileageStats_ServiceRecordsWithNullMileage_IgnoresNullsAndUsesOnlyValid()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = null,
                ServiceDate = new DateTime(2024, 1, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service2",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 15),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service3",
                Mileage = null,
                ServiceDate = new DateTime(2024, 2, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(1500);
        result.LastMileage.ShouldBe(1500);
        result.DistanceDriven.ShouldBe(0);
    }

    [Fact]
    public void CalculateMileageStats_AllServiceRecordsWithNullMileage_ReturnsZero()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = null,
                ServiceDate = new DateTime(2024, 1, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service2",
                Mileage = null,
                ServiceDate = new DateTime(2024, 1, 15),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(0);
        result.LastMileage.ShouldBe(0);
        result.DistanceDriven.ShouldBe(0);
    }

    [Fact]
    public void CalculateMileageStats_SingleEnergyEntry_ReturnsZeroDistance()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1500,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(1500);
        result.LastMileage.ShouldBe(1500);
        result.DistanceDriven.ShouldBe(0);
    }

    [Fact]
    public void CalculateMileageStats_SingleServiceRecord_ReturnsZeroDistance()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(1500);
        result.LastMileage.ShouldBe(1500);
        result.DistanceDriven.ShouldBe(0);
    }

    [Fact]
    public void CalculateMileageStats_HighMileageValues_CalculatesCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 999_000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1_500_000,
                ServiceDate = new DateTime(2024, 2, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(999_000);
        result.LastMileage.ShouldBe(1_500_000);
        result.DistanceDriven.ShouldBe(501_000);
    }

    [Fact]
    public void CalculateMileageStats_ZeroMileageValues_ReturnsZeroDistance()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 0,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 0,
                ServiceDate = new DateTime(2024, 2, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(0);
        result.LastMileage.ShouldBe(0);
        result.DistanceDriven.ShouldBe(0);
    }

    [Fact]
    public void CalculateMileageStats_FirstMileageZeroOnlyLastMileagePositive_ReturnsZeroDistance()
    {
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 0,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1000,
                ServiceDate = new DateTime(2024, 2, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(0);
        result.LastMileage.ShouldBe(1000);
        result.DistanceDriven.ShouldBe(0);
    }

    [Fact]
    public void CalculateMileageStats_LastMileageZeroOnlyFirstMileagePositive_ReturnsZeroDistance()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 0,
                ServiceDate = new DateTime(2024, 2, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(0);
        result.LastMileage.ShouldBe(1000);
        result.DistanceDriven.ShouldBe(0);
    }

    [Fact]
    public void CalculateMileageStats_LargeNumberOfEntries_PerformanceTest()
    {
        // Arrange - test with 1000 entries
        var energyEntries = Enumerable.Range(1, 500)
            .Select(i => new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = i * 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1).AddDays(i),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            })
            .ToList();

        var serviceRecords = Enumerable.Range(1, 500)
            .Select(i => new ServiceRecord
            {
                Id = Guid.NewGuid(),
                Title = $"Service {i}",
                Mileage = i * 500,
                ServiceDate = new DateTime(2024, 1, 1).AddDays(i),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            })
            .ToList();

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(500);
        result.LastMileage.ShouldBe(500_000);
        result.DistanceDriven.ShouldBe(499_500);
    }

    [Fact]
    public void CalculateMileageStats_WithMixedFuelTypes_CalculatesCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 45,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.LPG,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 40,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 4000,
                Volume = 70,
                Date = new DateOnly(2024, 2, 1),
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Cost = 15,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(1000);
        result.LastMileage.ShouldBe(4000);
        result.DistanceDriven.ShouldBe(3000);
    }

    [Fact]
    public void CalculateMileageStats_TwoEnergyEntriesWithDifferentFuelTypesAndTheSameMileage_CalculatesCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 25,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.LPG,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateMileageStats(energyEntries, serviceRecords);

        // Assert
        result.FirstMileage.ShouldBe(1000);
        result.LastMileage.ShouldBe(1000);
        result.DistanceDriven.ShouldBe(0);
    }

    #endregion

    #region CalculateCostStats Tests

    [Fact]
    public void CalculateCostStats_WithValidEnergyAndServiceCosts_CalculatesCostCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 10),
                ManualCost = 200,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service2",
                Mileage = 1800,
                ServiceDate = new DateTime(2024, 1, 20),
                ManualCost = 150,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(115); // 60 + 55
        result.TotalServiceCost.ShouldBe(350); // 200 + 150
        result.TotalCost.ShouldBe(465); // 115 + 350
        result.FuelCostPerKm.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CalculateCostStats_EmptyEnergyAndServiceEntries_ReturnsZeroCosts()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(0);
        result.TotalServiceCost.ShouldBe(0);
        result.TotalCost.ShouldBe(0);
        result.FuelCostPerKm.ShouldBe(0);
    }

    [Fact]
    public void CalculateCostStats_OnlyEnergyEntriesWithCosts_CalculatesFuelCostOnly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(115);
        result.TotalServiceCost.ShouldBe(0);
        result.TotalCost.ShouldBe(115);
    }

    [Fact]
    public void CalculateCostStats_OnlyServiceRecordsWithCosts_CalculatesServiceCostOnly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 10),
                ManualCost = 300,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service2",
                Mileage = 2000,
                ServiceDate = new DateTime(2024, 1, 20),
                ManualCost = 250,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(0);
        result.TotalServiceCost.ShouldBe(550);
        result.TotalCost.ShouldBe(550);
        result.FuelCostPerKm.ShouldBe(0);
    }

    [Fact]
    public void CalculateCostStats_EnergyEntriesWithNullCost_TreatsAsZero()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 75,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(75); // null treated as 0
        result.TotalServiceCost.ShouldBe(0);
        result.TotalCost.ShouldBe(75);
    }

    [Fact]
    public void CalculateCostStats_AllEnergyEntriesWithNullCost_ReturnsZeroFuelCost()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(0);
        result.TotalServiceCost.ShouldBe(0);
        result.TotalCost.ShouldBe(0);
    }

    [Fact]
    public void CalculateCostStats_ServiceRecordsWithZeroManualCostAndItems_CalculatesTotalFromItems()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 10),
                ManualCost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid(),
                Items = new List<ServiceItem>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Part1",
                        UnitPrice = 100,
                        Quantity = 2,
                        ServiceRecordId = Guid.NewGuid(),
                        Type = ServiceItemType.Other
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Labor",
                        UnitPrice = 50,
                        Quantity = 3,
                        ServiceRecordId = Guid.NewGuid(),
                        Type = ServiceItemType.Other
                    }
                }
            }
        };

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        // TotalCost property in ServiceRecord: s.ManualCost ?? s.Items.Sum(i => i.UnitPrice * i.Quantity)
        // = null ?? (100*2 + 50*3) = 350
        result.TotalServiceCost.ShouldBe(350);
        result.TotalCost.ShouldBe(350);
    }

    [Fact]
    public void CalculateCostStats_HighEnergyCosts_CalculatesCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 99999.99m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 88888.88m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(188888.87m);
        result.TotalCost.ShouldBe(188888.87m);
    }

    [Fact]
    public void CalculateCostStats_DecimalPrecision_MaintainsCurrencyAccuracy()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 10.99m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 15.49m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 10),
                ManualCost = 99.99m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(26.48m);
        result.TotalServiceCost.ShouldBe(99.99m);
        result.TotalCost.ShouldBe(126.47m);
    }

    [Fact]
    public void CalculateCostStats_ZeroCosts_ReturnsZeroTotals()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 0,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 10),
                ManualCost = 0,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(0);
        result.TotalServiceCost.ShouldBe(0);
        result.TotalCost.ShouldBe(0);
    }

    [Fact]
    public void CalculateCostStats_MixedNullAndValidCosts_SumsValidCostsOnly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1500,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 45.50m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2500,
                Volume = 50,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.LPG,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 32.75m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(78.25m); // 45.50 + 32.75
        result.TotalServiceCost.ShouldBe(0);
        result.TotalCost.ShouldBe(78.25m);
    }

    [Fact]
    public void CalculateCostStats_LargeNumberOfEntries_CalculatesCorrectly()
    {
        // Arrange - 1000 entries
        var energyEntries = Enumerable.Range(1, 500)
            .Select(i => new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = i * 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1).AddDays(i),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = i * 10, // 10, 20, 30, ... 5000
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            })
            .ToList();

        var serviceRecords = Enumerable.Range(1, 500)
            .Select(i => new ServiceRecord
            {
                Id = Guid.NewGuid(),
                Title = $"Service {i}",
                Mileage = i * 500,
                ServiceDate = new DateTime(2024, 1, 1).AddDays(i),
                ManualCost = i * 5, // 5, 10, 15, ... 2500
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            })
            .ToList();

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        // Sum of 1..500 = 500 * 501 / 2 = 125250
        var expectedFuelCost = 125250 * 10; // 1,252,500
        var expectedServiceCost = 125250 * 5; // 626,250
        result.TotalFuelCost.ShouldBe(expectedFuelCost);
        result.TotalServiceCost.ShouldBe(expectedServiceCost);
        result.TotalCost.ShouldBe(expectedFuelCost + expectedServiceCost);
    }

    [Fact]
    public void CalculateCostStats_SingleEnergyEntry_CanCalculateFuelCostPerKmZero()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(60);
        result.FuelCostPerKm.ShouldBe(0); // CalculateFuelCostPerDistanceUnit returns 0 for count < 2
    }

    [Fact]
    public void CalculateCostStats_ElectricEnergyEntries_CalculatesCostCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 60,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Cost = 15.50m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 60,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Cost = 15.50m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        result.TotalFuelCost.ShouldBe(31.00m);
        result.TotalCost.ShouldBe(31.00m);
    }

    [Fact]
    public void CalculateCostStats_MultipleServiceRecordsWithAndWithoutManualCost_CalculatesCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1000,
                ServiceDate = new DateTime(2024, 1, 10),
                ManualCost = 200,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid(),
                Items = []
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service2",
                Mileage = 2000,
                ServiceDate = new DateTime(2024, 1, 20),
                ManualCost = null, // Will use Items
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid(),
                Items = new List<ServiceItem>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Part",
                        UnitPrice = 150,
                        Quantity = 1,
                        ServiceRecordId = Guid.NewGuid(),
                        Type = ServiceItemType.Other
                    }
                }
            }
        };

        // Act
        var result = StatisticsService.CalculateCostStats(energyEntries, serviceRecords);

        // Assert
        // Service1: ManualCost = 200
        // Service2: Items sum = 150 * 1 = 150
        result.TotalServiceCost.ShouldBe(350);
        result.TotalCost.ShouldBe(350);
    }

    #endregion

    #region CalculateDateStats Tests

    [Fact]
    public void CalculateDateStats_WithValidEnergyAndServiceDates_ReturnsLatestDates()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 12),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service2",
                Mileage = 2500,
                ServiceDate = new DateTime(2024, 1, 18),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2024, 1, 20));
        result.LastServiceDate.ShouldBe(new DateOnly(2024, 1, 18));
    }

    [Fact]
    public void CalculateDateStats_EmptyEnergyAndServiceEntries_ReturnsNullDates()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBeNull();
        result.LastServiceDate.ShouldBeNull();
    }

    [Fact]
    public void CalculateDateStats_OnlyEnergyEntries_ReturnsOnlyFuelEntryDate()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 25),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2024, 1, 25));
        result.LastServiceDate.ShouldBeNull();
    }

    [Fact]
    public void CalculateDateStats_OnlyServiceRecords_ReturnsOnlyServiceDate()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 12),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service2",
                Mileage = 2500,
                ServiceDate = new DateTime(2024, 2, 5),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBeNull();
        result.LastServiceDate.ShouldBe(new DateOnly(2024, 2, 5));
    }

    [Fact]
    public void CalculateDateStats_SingleEnergyEntry_ReturnsSingleDate()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2024, 1, 15));
        result.LastServiceDate.ShouldBeNull();
    }

    [Fact]
    public void CalculateDateStats_SingleServiceRecord_ReturnsSingleDate()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 20),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBeNull();
        result.LastServiceDate.ShouldBe(new DateOnly(2024, 1, 20));
    }

    [Fact]
    public void CalculateDateStats_EnergyEntriesWithSameDate_ReturnsCorrectMaxDate()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.LPG,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2024, 1, 15));
    }

    [Fact]
    public void CalculateDateStats_DateTimeConversionFromDateTime_CorrectlyConvertsToDateOnly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 3, 15, 14, 30, 45),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastServiceDate.ShouldBe(new DateOnly(2024, 3, 15));
    }

    [Fact]
    public void CalculateDateStats_EarlyDates_CalculatesCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2000, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2000, 1, 1));
    }

    [Fact]
    public void CalculateDateStats_FutureDates_CalculatesCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2099, 12, 31),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2099, 12, 31));
    }

    [Fact]
    public void CalculateDateStats_MixedDatesAcrossYears_ReturnsMaxDate()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2022, 12, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 5),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 50,
                Date = new DateOnly(2023, 6, 20),
                Type = EnergyType.LPG,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2024, 1, 5));
    }

    [Fact]
    public void CalculateDateStats_LargeNumberOfEntries_PerformanceAndCorrectness()
    {
        // Arrange
        var energyEntries = Enumerable.Range(1, 500)
            .Select(i => new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = i * 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1).AddDays(i),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = i * 10,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            })
            .ToList();

        var serviceRecords = Enumerable.Range(1, 500)
            .Select(i => new ServiceRecord
            {
                Id = Guid.NewGuid(),
                Title = $"Service {i}",
                Mileage = i * 500,
                ServiceDate = new DateTime(2024, 1, 1).AddDays(i),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            })
            .ToList();

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2024, 1, 1).AddDays(500));
        result.LastServiceDate.ShouldBe(new DateOnly(2024, 1, 1).AddDays(500));
    }

    [Fact]
    public void CalculateDateStats_ServiceDateAtMidnight_ConvertedCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 15, 0, 0, 0),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastServiceDate.ShouldBe(new DateOnly(2024, 1, 15));
    }

    [Fact]
    public void CalculateDateStats_ServiceDateWithMilliseconds_ConvertedCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 15, 23, 59, 59, 999),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastServiceDate.ShouldBe(new DateOnly(2024, 1, 15));
    }

    [Fact]
    public void CalculateDateStats_ConsecutiveDaysInDifferentMonths_ReturnsCorrectMax()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 31),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 2, 1),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2024, 2, 1));
    }

    [Fact]
    public void CalculateDateStats_ConsecutiveDaysAcrossYears_ReturnsCorrectMax()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2023, 12, 31),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>();

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2024, 1, 1));
    }

    [Fact]
    public void CalculateDateStats_BothListsWithMultipleDates_ReturnsCorrectMaxForEach()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 3, 5),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 50,
                Date = new DateOnly(2024, 2, 15),
                Type = EnergyType.LPG,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };
        var serviceRecords = new List<ServiceRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service1",
                Mileage = 1500,
                ServiceDate = new DateTime(2024, 1, 20),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service2",
                Mileage = 2500,
                ServiceDate = new DateTime(2024, 2, 28),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Service3",
                Mileage = 3500,
                ServiceDate = new DateTime(2024, 4, 1),
                VehicleId = Guid.NewGuid(),
                Vehicle = null!,
                TypeId = Guid.NewGuid()
            }
        };

        // Act
        var result = StatisticsService.CalculateDateStats(energyEntries, serviceRecords);

        // Assert
        result.LastFuelEntryDate.ShouldBe(new DateOnly(2024, 3, 5));
        result.LastServiceDate.ShouldBe(new DateOnly(2024, 4, 1));
    }

    #endregion

    #region CalculateEfficiencyStatsByEnergyType Tests

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_EmptyEnergyEntries_ReturnsEmptyList()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>();

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_SingleEnergyType_ReturnsSingleStatWithCorrectValues()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(1);
        result[0].FuelType.ShouldBe(EnergyType.Gasoline);
        result[0].EnergyUnit.ShouldBe("Liter");
        result[0].TotalCost.ShouldBe(115);
        result[0].EntriesCount.ShouldBe(2);
        result[0].AverageConsumption.ShouldBeGreaterThanOrEqualTo(0);
        result[0].CostPerKm.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_MultipleEnergyTypes_ReturnsMultipleStats()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 40,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 4000,
                Volume = 40,
                Date = new DateOnly(2024, 2, 1),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 48,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(x => x.FuelType == EnergyType.Gasoline);
        result.ShouldContain(x => x.FuelType == EnergyType.Diesel);

        var gasolineStat = result.First(x => x.FuelType == EnergyType.Gasoline);
        gasolineStat.TotalCost.ShouldBe(115);
        gasolineStat.EntriesCount.ShouldBe(2);

        var dieselStat = result.First(x => x.FuelType == EnergyType.Diesel);
        dieselStat.TotalCost.ShouldBe(98);
        dieselStat.EntriesCount.ShouldBe(2);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_SingleEntryPerType_ReturnsZeroAverageConsumptionAndCostPerKm()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(1);
        result[0].AverageConsumption.ShouldBe(0);
        result[0].CostPerKm.ShouldBe(0);
        result[0].TotalCost.ShouldBe(60);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_EnergyTypeWithNullCosts_TreatsAsZero()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 75,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(1);
        result[0].TotalCost.ShouldBe(75);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_AllEnergyTypesWithNullCosts_ReturnsZeroTotalCost()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(1);
        result[0].TotalCost.ShouldBe(0);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_Electric_UsesKWhUnit()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 60,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Cost = 15,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 60,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Cost = 15,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(1);
        result[0].FuelType.ShouldBe(EnergyType.Electric);
        result[0].EnergyUnit.ShouldBe("kWh");
        result[0].TotalCost.ShouldBe(30);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_MixedUnitsForDifferentTypes_EachGroupHasOwnUnit()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 60,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Cost = 15,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 4000,
                Volume = 60,
                Date = new DateOnly(2024, 2, 1),
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Cost = 15,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(2);
        result.First(x => x.FuelType == EnergyType.Gasoline).EnergyUnit.ShouldBe("Liter");
        result.First(x => x.FuelType == EnergyType.Electric).EnergyUnit.ShouldBe("kWh");
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_AllEnergyTypes_ReturnsCorrectGroups()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 5),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 45,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.LPG,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 40,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 4000,
                Volume = 40,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.CNG,
                EnergyUnit = EnergyUnit.CubicMeter,
                Cost = 30,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 5000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.Ethanol,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 65,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 6000,
                Volume = 48,
                Date = new DateOnly(2024, 1, 25),
                Type = EnergyType.Biofuel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 58,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 7000,
                Volume = 3,
                Date = new DateOnly(2024, 2, 1),
                Type = EnergyType.Hydrogen,
                EnergyUnit = EnergyUnit.CubicMeter,
                Cost = 40,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 8000,
                Volume = 60,
                Date = new DateOnly(2024, 2, 5),
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Cost = 15,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(8);
        result.ShouldContain(x => x.FuelType == EnergyType.Gasoline);
        result.ShouldContain(x => x.FuelType == EnergyType.Diesel);
        result.ShouldContain(x => x.FuelType == EnergyType.LPG);
        result.ShouldContain(x => x.FuelType == EnergyType.CNG);
        result.ShouldContain(x => x.FuelType == EnergyType.Ethanol);
        result.ShouldContain(x => x.FuelType == EnergyType.Biofuel);
        result.ShouldContain(x => x.FuelType == EnergyType.Hydrogen);
        result.ShouldContain(x => x.FuelType == EnergyType.Electric);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_HighCostValues_CalculatesCorrectly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 99999.99m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 88888.88m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(1);
        result[0].TotalCost.ShouldBe(188888.87m);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_LargeNumberOfEntriesPerType_CalculatesCorrectly()
    {
        // Arrange
        var energyEntries = Enumerable.Range(1, 100)
            .Select(i => new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = i * 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1).AddDays(i),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = i * 10,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            })
            .ToList();

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(1);

        result[0].TotalCost.ShouldBe(50500);
        result[0].EntriesCount.ShouldBe(100);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_ManyTypesWithFewEntries_ReturnsCorrectCount()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 45,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.LPG,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 40,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(3);
        result.All(x => x.EntriesCount == 1).ShouldBeTrue();
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_ZeroCosts_ReturnsZeroTotalCost()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 0,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 0,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(1);
        result[0].TotalCost.ShouldBe(0);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_MixedNullAndValidCosts_SumsValidOnly()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1500,
                Volume = 50,
                Date = new DateOnly(2024, 1, 8),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 45.50m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = null,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2500,
                Volume = 50,
                Date = new DateOnly(2024, 1, 22),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 52.75m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(1);
        result[0].TotalCost.ShouldBe(98.25m);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_DecimalPrecision_MaintainsCurrencyAccuracy()
    {
        // Arrange
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 10.99m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 15.49m,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(1);
        result[0].TotalCost.ShouldBe(26.48m);
    }

    [Fact]
    public void CalculateEfficiencyStatsByEnergyType_VerifiesEachGroupUsesDependentCalculations()
    {
        var energyEntries = new List<EnergyEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new()
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.Diesel,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateEfficiencyStatsByEnergyType(energyEntries);

        // Assert
        result.Count.ShouldBe(2);

        var gasolineStat = result.First(x => x.FuelType == EnergyType.Gasoline);
        gasolineStat.AverageConsumption.ShouldBeGreaterThan(0);
        gasolineStat.CostPerKm.ShouldBeGreaterThan(0);

        var dieselStat = result.First(x => x.FuelType == EnergyType.Diesel);
        dieselStat.AverageConsumption.ShouldBe(0);
        dieselStat.CostPerKm.ShouldBe(0);
    }

    #endregion

    #region CalculateAverageConsumption Tests

    [Fact]
    public void CalculateAverageConsumption_TwoEntriesWithValidData_CalculatesConsumptionCorrectly()
    {
        // Arrange - 2 entries: 50L at 1000km, 50L at 2000km
        // Consumed: 50L (first entry), Distance: 1000km
        // Consumption: (50 / 1000) * 100 = 5.0 l/100km
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(5.0);
    }

    [Fact]
    public void CalculateAverageConsumption_EmptyList_ReturnsZero()
    {
        // Arrange
        var entries = new List<EnergyEntry>();

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public void CalculateAverageConsumption_SingleEntry_ReturnsZero()
    {
        // Arrange
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public void CalculateAverageConsumption_ThreeEntries_IgnoresLastEntry()
    {
        // Arrange - 3 entries: 50L, 50L, 50L at 1000, 2000, 3000km
        // Consumed: 50 + 50 (ignore last) = 100L, Distance: 2000km
        // Consumption: (100 / 2000) * 100 = 5.0 l/100km
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(5.0);
    }

    [Fact]
    public void CalculateAverageConsumption_UnsortedEntries_SortsByMileageCorrectly()
    {
        // Arrange - entries NOT sorted by mileage
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 50,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(5.0);
    }

    [Fact]
    public void CalculateAverageConsumption_ZeroDistance_ReturnsZero()
    {
        // Arrange - same mileage
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public void CalculateAverageConsumption_DifferentVolumes_SumsCorrectly()
    {
        // Arrange - different volumes: 50L, 40L, 45L at 1000, 2000, 3000km
        // Consumed: 50 + 40 = 90L, Distance: 2000km
        // Consumption: (90 / 2000) * 100 = 4.5 l/100km
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 40,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 48,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 45,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 52,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(4.5);
    }

    [Fact]
    public void CalculateAverageConsumption_DecimalVolumes_CalculatesWithPrecision()
    {
        // Arrange - decimal volumes: 50.5L, 49.75L at 1000, 2000km
        // Consumed: 50.5L, Distance: 1000km
        // Consumption: (50.5 / 1000) * 100 = 5.05 l/100km
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50.5m,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 49.75m,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(5.05);
    }

    [Fact]
    public void CalculateAverageConsumption_RoundingToTwoDecimalPlaces_WorksCorrectly()
    {
        // Arrange - values that need rounding: 50L, 40L at 1000, 2333km
        // Consumed: 50L, Distance: 1333km
        // Consumption: (50 / 1333) * 100 = 3.75187... → 3.75
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 2333,
                Volume = 40,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(3.75);
    }

    [Fact]
    public void CalculateAverageConsumption_LargeDistance_CalculatesCorrectly()
    {
        // Arrange - large distance: 50L at 1000km to 100000km
        // Consumed: 50L, Distance: 99000km
        // Consumption: (50 / 99000) * 100 = 0.05 l/100km
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 100000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 55,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(0.05);
    }

    [Fact]
    public void CalculateAverageConsumption_SmallVolume_CalculatesCorrectly()
    {
        // Arrange - small volumes: 0.5L at 1000, 2000km
        // Consumed: 0.5L, Distance: 1000km
        // Consumption: (0.5 / 1000) * 100 = 0.05 l/100km
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 0.5m,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 1,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 0.5m,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 1,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(0.05);
    }

    [Fact]
    public void CalculateAverageConsumption_HighConsumption_CalculatesCorrectly()
    {
        // Arrange - high consumption: 100L, 100L at 1000, 2000km
        // Consumed: 100L, Distance: 1000km
        // Consumption: (100 / 1000) * 100 = 10.0 l/100km
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 100,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 120,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 100,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 120,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(10.0);
    }

    [Fact]
    public void CalculateAverageConsumption_ElectricEnergyWithkWh_CalculatesConsumptionCorrectly()
    {
        // Arrange - Electric vehicle with kWh instead of Liter
        // 60 kWh, 60 kWh at 1000, 2000km
        // Consumed: 60 kWh, Distance: 1000km
        // Consumption: (60 / 1000) * 100 = 6.0 kWh/100km
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 60,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Cost = 15,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 60,
                Date = new DateOnly(2024, 1, 15),
                Type = EnergyType.Electric,
                EnergyUnit = EnergyUnit.kWh,
                Cost = 15,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(6.0);
    }

    [Fact]
    public void CalculateAverageConsumption_ManyEntries_IgnoresOnlyLastEntry()
    {
        // Arrange - 5 entries: 50L, 50L, 50L, 50L, 50L at 1000, 2000, 3000, 4000, 5000km
        // Consumed: 50 + 50 + 50 + 50 (ignore last) = 200L, Distance: 4000km
        // Consumption: (200 / 4000) * 100 = 5.0 l/100km
        var entries = Enumerable.Range(1, 5)
            .Select(i => new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = i * 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, i),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            })
            .ToList();

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(5.0);
    }

    [Fact]
    public void CalculateAverageConsumption_VariousVolumesInManyEntries_CalculatesCorrectly()
    {
        // Arrange - 4 entries: 60L, 50L, 45L, 55L at 1000, 2000, 3000, 4000km
        // Consumed: 60 + 50 + 45 = 155L, Distance: 3000km
        // Consumption: (155 / 3000) * 100 = 5.17 l/100km
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 60,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 72,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 2000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 3000,
                Volume = 45,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 54,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 4000,
                Volume = 55,
                Date = new DateOnly(2024, 2, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 66,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        result.ShouldBe(5.17);
    }

    [Fact]
    public void CalculateAverageConsumption_LargeNumberOfEntries_CalculatesCorrectly()
    {
        // Arrange - 100 entries, 50L each
        var entries = Enumerable.Range(1, 100)
            .Select(i => new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = i * 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 1).AddDays(i),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            })
            .ToList();

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        // Consumed: 50 * 99 = 4950L, Distance: 99000km
        // Consumption: (4950 / 99000) * 100 = 5.0
        result.ShouldBe(5.0);
    }

    [Fact]
    public void CalculateAverageConsumption_FirstAndLastAtSpecialMileages_CalculatesFromCorrectRange()
    {
        // Arrange - entries at specific mileages
        var entries = new List<EnergyEntry>
        {
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 0,
                Volume = 60,
                Date = new DateOnly(2024, 1, 1),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 72,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 500,
                Volume = 50,
                Date = new DateOnly(2024, 1, 10),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            },
            new EnergyEntry
            {
                Id = Guid.NewGuid(),
                Mileage = 1000,
                Volume = 50,
                Date = new DateOnly(2024, 1, 20),
                Type = EnergyType.Gasoline,
                EnergyUnit = EnergyUnit.Liter,
                Cost = 60,
                VehicleId = Guid.NewGuid(),
                Vehicle = null!
            }
        };

        // Act
        var result = StatisticsService.CalculateAverageConsumption(entries);

        // Assert
        // Consumed: 60 + 50 = 110L, Distance: 1000km
        // Consumption: (110 / 1000) * 100 = 11.0
        result.ShouldBe(11.0);
    }

    #endregion
}