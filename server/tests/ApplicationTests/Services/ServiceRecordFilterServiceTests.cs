using Application.Features.ServiceRecords.Get;
using Application.Services;
using Domain.Entities.Services;
using Domain.Enums.Services;
using Shouldly;
using Xunit;

namespace ApplicationTests.Services;

public class ServiceRecordFilterServiceTests
{
    private readonly ServiceRecordFilterService _service;

    public ServiceRecordFilterServiceTests()
    {
        _service = new ServiceRecordFilterService();
    }

    #region ApplyFilters Tests - SearchTerm

    [Fact]
    public void ApplyFilters_WhenSearchTermIsNull_ReturnsUnfilteredQuery()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: null);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(5);
    }

    [Fact]
    public void ApplyFilters_WhenSearchTermIsEmpty_ReturnsUnfilteredQuery()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "");

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(5);
    }

    [Fact]
    public void ApplyFilters_WhenSearchTermIsWhitespace_ReturnsUnfilteredQuery()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "   ");

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(5);
    }

    [Fact]
    public void ApplyFilters_WhenSearchTermMatchesTitle_ReturnsMatchingRecords()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "Oil");

        // Act
        var result = _service.ApplyFilters(query, request).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.All(r => r.Title.Contains("Oil")).ShouldBeTrue();
    }

    [Fact]
    public void ApplyFilters_WhenSearchTermMatchesNotes_ReturnsMatchingRecords()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "synthetic");

        // Act
        var result = _service.ApplyFilters(query, request).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Notes?.ShouldContain("synthetic");
    }

    [Fact]
    public void ApplyFilters_WhenSearchTermMatchesTitleOrNotes_ReturnsAllMatching()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "Change");

        // Act
        var result = _service.ApplyFilters(query, request).ToList();

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public void ApplyFilters_WhenSearchTermHasNoMatch_ReturnsEmptyQuery()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "nonexistent");

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(0);
    }

    [Fact]
    public void ApplyFilters_WhenSearchTermIsUpperCase_IsCaseInsensitive()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "OIL");

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(2);
    }

    [Fact]
    public void ApplyFilters_WhenNotesIsNull_DoesNotThrowException()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithNullNotes();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "test");

        // Act & Assert
        Should.NotThrow(() => _service.ApplyFilters(query, request).ToList());
    }

    #endregion

    #region ApplyFilters Tests - Type

    [Fact]
    public void ApplyFilters_WhenServiceTypeIsNull_ReturnsUnfilteredQuery()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, Type: null);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(5);
    }

    [Fact]
    public void ApplyFilters_WhenServiceTypeIsDefined_ReturnsMatchingRecords()
    {
        // Arrange
        var targetType = ServiceRecordType.OilAndFilters;
        var records = CreateTestServiceRecordsWithType(targetType);
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, Type: targetType);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        var filtered = result.ToList();
        filtered.Count.ShouldBe(3);
        filtered.All(r => r.Type == targetType).ShouldBeTrue();
    }

    [Fact]
    public void ApplyFilters_WhenServiceTypeHasNoMatch_ReturnsEmptyQuery()
    {
        // Arrange
        var targetType = ServiceRecordType.OilAndFilters;
        var records = CreateTestServiceRecordsWithType(targetType);
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, Type: ServiceRecordType.Electrical);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(0);
    }

    #endregion

    #region ApplyFilters Tests - DateFrom

    [Fact]
    public void ApplyFilters_WhenDateFromIsNull_ReturnsUnfilteredQuery()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, DateFrom: null);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(5);
    }

    [Fact]
    public void ApplyFilters_WhenDateFromIsProvided_ReturnsRecordsOnOrAfterDate()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();
        var dateFrom = new DateTime(2024, 6, 1);
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, DateFrom: dateFrom);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        var filtered = result.ToList();
        filtered.Count.ShouldBe(3);
        filtered.All(r => r.ServiceDate >= dateFrom).ShouldBeTrue();
    }

    [Fact]
    public void ApplyFilters_WhenDateFromEqualsServiceDate_IncludesRecord()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();
        var dateFrom = new DateTime(2024, 5, 15);
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, DateFrom: dateFrom);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        var filtered = result.ToList();
        filtered.Any(r => r.ServiceDate == dateFrom).ShouldBeTrue();
    }

    [Fact]
    public void ApplyFilters_WhenDateFromIsInFuture_ReturnsEmptyQuery()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();
        var dateFrom = new DateTime(2025, 1, 1);
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, DateFrom: dateFrom);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(0);
    }

    #endregion

    #region ApplyFilters Tests - DateTo

    [Fact]
    public void ApplyFilters_WhenDateToIsNull_ReturnsUnfilteredQuery()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, DateTo: null);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(5);
    }

    [Fact]
    public void ApplyFilters_WhenDateToIsProvided_ReturnsRecordsOnOrBeforeDate()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();
        var dateTo = new DateTime(2024, 5, 31);
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, DateTo: dateTo);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        var filtered = result.ToList();
        filtered.Count.ShouldBe(2);
        filtered.All(r => r.ServiceDate <= dateTo).ShouldBeTrue();
    }

    [Fact]
    public void ApplyFilters_WhenDateToEqualsServiceDate_IncludesRecord()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();
        var dateTo = new DateTime(2024, 5, 15);
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, DateTo: dateTo);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        var filtered = result.ToList();
        filtered.Any(r => r.ServiceDate == dateTo).ShouldBeTrue();
    }

    [Fact]
    public void ApplyFilters_WhenDateToIsInPast_ReturnsEmptyQuery()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();
        var dateTo = new DateTime(2024, 1, 1);
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, DateTo: dateTo);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(0);
    }

    #endregion

    #region ApplyFilters Tests - Combined Filters

    [Fact]
    public void ApplyFilters_WhenDateFromAndDateToProvided_ReturnsRecordsInRange()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();
        var dateFrom = new DateTime(2024, 5, 1);
        var dateTo = new DateTime(2024, 6, 30);
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, DateFrom: dateFrom, DateTo: dateTo);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        var filtered = result.ToList();
        filtered.Count.ShouldBe(2);
        filtered.All(r => r.ServiceDate >= dateFrom && r.ServiceDate <= dateTo).ShouldBeTrue();
    }

    [Fact]
    public void ApplyFilters_WhenAllFiltersProvided_ReturnsRecordsMatchingAllCriteria()
    {
        // Arrange
        var targetType = ServiceRecordType.OilAndFilters;
        var records = CreateTestServiceRecordsForCombinedFilters(targetType);
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(
            Guid.NewGuid(),
            1,
            10,
            SearchTerm: "Oil",
            Type: targetType,
            DateFrom: new DateTime(2024, 5, 1),
            DateTo: new DateTime(2024, 6, 30));

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        var filtered = result.ToList();
        filtered.Count.ShouldBe(1);
        filtered[0].Title.ShouldContain("Oil");
        filtered[0].Type.ShouldBe(targetType);
    }

    [Fact]
    public void ApplyFilters_WhenNoFiltersProvided_ReturnsAllRecords()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(5);
    }

    #endregion

    #region ApplySorting Tests - ServiceDate

    [Fact]
    public void ApplySorting_WhenSortByIsNull_SortsByServiceDateDescending()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, null, false);

        // Assert
        var sorted = result.ToList();
        sorted[0].ServiceDate.ShouldBe(new DateTime(2024, 8, 1));
        sorted[4].ServiceDate.ShouldBe(new DateTime(2024, 3, 1));
    }

    [Fact]
    public void ApplySorting_WhenSortByServiceDateDescending_SortsCorrectly()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "servicedate", true);

        // Assert
        var sorted = result.ToList();
        sorted[0].ServiceDate.ShouldBe(new DateTime(2024, 8, 1));
        sorted[4].ServiceDate.ShouldBe(new DateTime(2024, 3, 1));
    }

    [Fact]
    public void ApplySorting_WhenSortByServiceDateAscending_SortsCorrectly()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "servicedate", false);

        // Assert
        var sorted = result.ToList();
        sorted[0].ServiceDate.ShouldBe(new DateTime(2024, 3, 1));
        sorted[4].ServiceDate.ShouldBe(new DateTime(2024, 8, 1));
    }

    #endregion

    #region ApplySorting Tests - Mileage

    [Fact]
    public void ApplySorting_WhenSortByMileageDescending_SortsCorrectly()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithMileage();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "mileage", true);

        // Assert
        var sorted = result.ToList();
        sorted[0].Mileage.ShouldBe(5000);
        sorted[4].Mileage.ShouldBe(500);
    }

    #endregion

    #region ApplySorting Tests - Title

    [Fact]
    public void ApplySorting_WhenSortByTitleAscending_SortsCorrectly()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithTitles();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "title", false);

        // Assert
        var sorted = result.ToList();
        sorted[0].Title.ShouldBe("Air Filter Change");
        sorted[4].Title.ShouldBe("Tire Rotation");
    }

    #endregion

    #region ApplySorting Tests - TotalCost

    [Fact]
    public void ApplySorting_WhenSortByTotalCostDescending_FallsBackAndRequiresInMemorySorting()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithCosts();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "totalcost", true);

        // Assert
        var sorted = result.ToList();
        var expectedByServiceDateDesc = records.OrderByDescending(r => r.ServiceDate).Select(r => r.Id).ToList();
        sorted.Select(r => r.Id).ShouldBe(expectedByServiceDateDesc);
        _service.RequiresInMemorySorting("totalcost").ShouldBeTrue();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ApplyFilters_WhenQueryIsEmpty_ReturnsEmptyQuery()
    {
        var records = new List<ServiceRecord>();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "test");
        var result = _service.ApplyFilters(query, request);
        result.Count().ShouldBe(0);
    }

    #endregion

    #region Helper Methods

    private static List<ServiceRecord> CreateTestServiceRecords()
    {
        return
        [
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m, "Regular oil change", ServiceRecordType.OilAndFilters),
            CreateServiceRecord("Tire Rotation", new DateTime(2024, 6, 1), 1200, 75m, "Rotated all tires", ServiceRecordType.Tires),
            CreateServiceRecord("Brake Inspection", new DateTime(2024, 7, 1), 1500, 200m, "Checked brake pads", ServiceRecordType.Brakes),
            CreateServiceRecord("Battery Replacement", new DateTime(2024, 8, 1), 1800, 300m, "New battery installed", ServiceRecordType.Electrical),
            CreateServiceRecord("Oil Filter Change", new DateTime(2024, 3, 1), 950, 50m, "synthetic", ServiceRecordType.OilAndFilters)
        ];
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithNullNotes()
    {
        return
        [
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m, null),
            CreateServiceRecord("Tire Rotation", new DateTime(2024, 6, 1), 1200, 75m, null)
        ];
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithType(ServiceRecordType type)
    {
        var otherType = type == ServiceRecordType.OilAndFilters ? ServiceRecordType.Tires : ServiceRecordType.OilAndFilters;

        return
        [
            CreateServiceRecord("R1", new DateTime(2024, 5, 15), 1000, 150m, type: type),
            CreateServiceRecord("R2", new DateTime(2024, 6, 1), 1200, 150m, type: type),
            CreateServiceRecord("R3", new DateTime(2024, 7, 1), 1500, 150m, type: type),
            CreateServiceRecord("R4", new DateTime(2024, 8, 1), 1800, 75m, type: otherType),
            CreateServiceRecord("R5", new DateTime(2024, 3, 1), 950, 50m, type: otherType)
        ];
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithDates()
    {
        return
        [
            CreateServiceRecord("Service 1", new DateTime(2024, 3, 1), 1000, 100m),
            CreateServiceRecord("Service 2", new DateTime(2024, 5, 15), 1200, 150m),
            CreateServiceRecord("Service 3", new DateTime(2024, 6, 1), 1500, 200m),
            CreateServiceRecord("Service 4", new DateTime(2024, 7, 1), 1800, 250m),
            CreateServiceRecord("Service 5", new DateTime(2024, 8, 1), 2000, 300m)
        ];
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithMileage()
    {
        return
        [
            CreateServiceRecord("Service 1", DateTime.Now, 1000, 100m),
            CreateServiceRecord("Service 2", DateTime.Now, 3000, 150m),
            CreateServiceRecord("Service 3", DateTime.Now, 500, 200m),
            CreateServiceRecord("Service 4", DateTime.Now, 2000, 250m),
            CreateServiceRecord("Service 5", DateTime.Now, 5000, 300m)
        ];
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithTitles()
    {
        return
        [
            CreateServiceRecord("Oil Change", DateTime.Now, 1000, 100m),
            CreateServiceRecord("Tire Rotation", DateTime.Now, 1200, 150m),
            CreateServiceRecord("Brake Inspection", DateTime.Now, 1500, 200m),
            CreateServiceRecord("Air Filter Change", DateTime.Now, 1800, 250m),
            CreateServiceRecord("Battery Replacement", DateTime.Now, 2000, 300m)
        ];
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithCosts()
    {
        return
        [
            CreateServiceRecord("Service 1", new DateTime(2024, 5, 15), 1000, 150m),
            CreateServiceRecord("Service 2", new DateTime(2024, 5, 16), 1200, 300m),
            CreateServiceRecord("Service 3", new DateTime(2024, 5, 17), 1500, 100m),
            CreateServiceRecord("Service 4", new DateTime(2024, 5, 18), 1800, 200m),
            CreateServiceRecord("Service 5", new DateTime(2024, 5, 19), 2000, 500m)
        ];
    }

    private static List<ServiceRecord> CreateTestServiceRecordsForCombinedFilters(ServiceRecordType type)
    {
        return
        [
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m, "Premium oil", type),
            CreateServiceRecord("Oil Filter", new DateTime(2024, 3, 1), 950, 50m, "Standard oil", type),
            CreateServiceRecord("Tire Rotation", new DateTime(2024, 6, 1), 1200, 75m, "All season tires", ServiceRecordType.Tires)
        ];
    }

    private static ServiceRecord CreateServiceRecord(
        string title,
        DateTime serviceDate,
        int? mileage,
        decimal cost,
        string? notes = null,
        ServiceRecordType type = ServiceRecordType.Other)
    {
        return new ServiceRecord
        {
            Id = Guid.NewGuid(),
            Title = title,
            ServiceDate = serviceDate,
            Mileage = mileage,
            ManualCost = cost,
            Notes = notes,
            VehicleId = Guid.NewGuid(),
            Type = type
        };
    }

    #endregion
}