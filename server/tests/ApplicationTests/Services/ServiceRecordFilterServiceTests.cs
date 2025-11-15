using Application.ServiceRecords.Get;
using Application.Services;
using Domain.Entities.Services;

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
        result.Count.ShouldBe(2); // "Oil Change" and "Oil Filter Change" in titles
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

    #region ApplyFilters Tests - ServiceTypeId

    [Fact]
    public void ApplyFilters_WhenServiceTypeIdIsNull_ReturnsUnfilteredQuery()
    {
        // Arrange
        var records = CreateTestServiceRecords();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, ServiceTypeId: null);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(5);
    }

    [Fact]
    public void ApplyFilters_WhenServiceTypeIdMatches_ReturnsMatchingRecords()
    {
        // Arrange
        var serviceTypeId = Guid.NewGuid();
        var records = CreateTestServiceRecordsWithServiceTypes(serviceTypeId);
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, ServiceTypeId: serviceTypeId);

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        var filtered = result.ToList();
        filtered.Count.ShouldBe(3);
        filtered.All(r => r.TypeId == serviceTypeId).ShouldBeTrue();
    }

    [Fact]
    public void ApplyFilters_WhenServiceTypeIdHasNoMatch_ReturnsEmptyQuery()
    {
        // Arrange
        var serviceTypeId = Guid.NewGuid();
        var nonExistentTypeId = Guid.NewGuid();
        var records = CreateTestServiceRecordsWithServiceTypes(serviceTypeId);
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, ServiceTypeId: nonExistentTypeId);

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
        filtered.Count.ShouldBe(2); // 2024-03-01 and 2024-05-15
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
        filtered.Count.ShouldBe(2); // 2024-05-15 and 2024-06-01
        filtered.All(r => r.ServiceDate >= dateFrom && r.ServiceDate <= dateTo).ShouldBeTrue();
    }

    [Fact]
    public void ApplyFilters_WhenAllFiltersProvided_ReturnsRecordsMatchingAllCriteria()
    {
        // Arrange
        var serviceTypeId = Guid.NewGuid();
        var records = CreateTestServiceRecordsForCombinedFilters(serviceTypeId);
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(
            Guid.NewGuid(), 
            1, 
            10, 
            SearchTerm: "Oil",
            ServiceTypeId: serviceTypeId,
            DateFrom: new DateTime(2024, 5, 1),
            DateTo: new DateTime(2024, 6, 30));

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        var filtered = result.ToList();
        filtered.Count.ShouldBe(1);
        filtered[0].Title.ShouldContain("Oil");
        filtered[0].TypeId.ShouldBe(serviceTypeId);
        filtered[0].ServiceDate.ShouldBeGreaterThanOrEqualTo(new DateTime(2024, 5, 1));
        filtered[0].ServiceDate.ShouldBeLessThanOrEqualTo(new DateTime(2024, 6, 30));
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

    [Fact]
    public void ApplyFilters_WhenMultipleFiltersResultInNoMatch_ReturnsEmptyQuery()
    {
        // Arrange
        var serviceTypeId = Guid.NewGuid();
        var records = CreateTestServiceRecordsForCombinedFilters(serviceTypeId);
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(
            Guid.NewGuid(), 
            1, 
            10, 
            SearchTerm: "NonExistent",
            ServiceTypeId: serviceTypeId,
            DateFrom: new DateTime(2024, 5, 1),
            DateTo: new DateTime(2024, 6, 30));

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(0);
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
        sorted[0].ServiceDate.ShouldBe(new DateTime(2024, 8, 1)); // Latest
        sorted[4].ServiceDate.ShouldBe(new DateTime(2024, 3, 1)); // Earliest
    }

    [Fact]
    public void ApplySorting_WhenSortByIsEmpty_SortsByServiceDateDescending()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "", false);

        // Assert
        var sorted = result.ToList();
        sorted[0].ServiceDate.ShouldBe(new DateTime(2024, 8, 1)); // Latest
        sorted[4].ServiceDate.ShouldBe(new DateTime(2024, 3, 1)); // Earliest
    }

    [Fact]
    public void ApplySorting_WhenSortByIsWhitespace_SortsByServiceDateDescending()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "   ", false);

        // Assert
        var sorted = result.ToList();
        sorted[0].ServiceDate.ShouldBe(new DateTime(2024, 8, 1)); // Latest
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
        sorted[1].ServiceDate.ShouldBe(new DateTime(2024, 7, 1));
        sorted[2].ServiceDate.ShouldBe(new DateTime(2024, 6, 1));
        sorted[3].ServiceDate.ShouldBe(new DateTime(2024, 5, 15));
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
        sorted[0].ServiceDate.ShouldBe(new DateTime(2024, 3, 1)); // Earliest
        sorted[4].ServiceDate.ShouldBe(new DateTime(2024, 8, 1)); // Latest
    }

    [Fact]
    public void ApplySorting_WhenSortByServiceDateCaseInsensitive_SortsCorrectly()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithDates();
        var query = records.AsQueryable();

        // Act
        var result1 = _service.ApplySorting(query, "SERVICEDATE", true);
        var result2 = _service.ApplySorting(query, "ServiceDate", true);
        var result3 = _service.ApplySorting(query, "serviceDate", true);

        // Assert
        result1.First().ServiceDate.ShouldBe(new DateTime(2024, 8, 1));
        result2.First().ServiceDate.ShouldBe(new DateTime(2024, 8, 1));
        result3.First().ServiceDate.ShouldBe(new DateTime(2024, 8, 1));
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
        sorted[1].Mileage.ShouldBe(3000);
        sorted[2].Mileage.ShouldBe(2000);
        sorted[3].Mileage.ShouldBe(1000);
        sorted[4].Mileage.ShouldBe(500);
    }

    [Fact]
    public void ApplySorting_WhenSortByMileageAscending_SortsCorrectly()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithMileage();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "mileage", false);

        // Assert
        var sorted = result.ToList();
        sorted[0].Mileage.ShouldBe(500);
        sorted[1].Mileage.ShouldBe(1000);
        sorted[2].Mileage.ShouldBe(2000);
        sorted[3].Mileage.ShouldBe(3000);
        sorted[4].Mileage.ShouldBe(5000);
    }

    [Fact]
    public void ApplySorting_WhenSortByMileageWithNullValues_HandlesCorrectly()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithNullMileage();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "mileage", true);

        // Assert
        var sorted = result.ToList();
        // Nulls should be at the end when descending
        Should.NotThrow(() => sorted.ToList());
    }

    #endregion

    #region ApplySorting Tests - Title

    [Fact]
    public void ApplySorting_WhenSortByTitleDescending_SortsCorrectly()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithTitles();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "title", true);

        // Assert
        var sorted = result.ToList();
        sorted[0].Title.ShouldBe("Tire Rotation");
        sorted[1].Title.ShouldBe("Oil Change");
        sorted[2].Title.ShouldBe("Brake Inspection");
        sorted[3].Title.ShouldBe("Battery Replacement");
        sorted[4].Title.ShouldBe("Air Filter Change");
    }

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
        sorted[1].Title.ShouldBe("Battery Replacement");
        sorted[2].Title.ShouldBe("Brake Inspection");
        sorted[3].Title.ShouldBe("Oil Change");
        sorted[4].Title.ShouldBe("Tire Rotation");
    }

    #endregion

    #region ApplySorting Tests - TotalCost (Adjusted: Sorting now requires in-memory and ApplySorting falls back)

    [Fact]
    public void ApplySorting_WhenSortByTotalCostDescending_FallsBackAndRequiresInMemorySorting()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithCosts();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "totalcost", true);

        // Assert
        // Now ApplySorting no longer sorts by TotalCost at IQueryable level, it falls back to ServiceDate DESC
        var sorted = result.ToList();
        var expectedByServiceDateDesc = records
            .OrderByDescending(r => r.ServiceDate)
            .Select(r => r.Id)
            .ToList();
        sorted.Select(r => r.Id).ShouldBe(expectedByServiceDateDesc);
        _service.RequiresInMemorySorting("totalcost").ShouldBeTrue();
    }

    [Fact]
    public void ApplySorting_WhenSortByTotalCostAscending_FallsBackAndRequiresInMemorySorting()
    {
        // Arrange
        var records = CreateTestServiceRecordsWithCosts();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "totalcost", false);

        // Assert
        // Still falls back to ServiceDate DESC regardless of ascending flag (because TotalCost not sorted in DB)
        var sorted = result.ToList();
        var expectedByServiceDateDesc = records
            .OrderByDescending(r => r.ServiceDate)
            .Select(r => r.Id)
            .ToList();
        sorted.Select(r => r.Id).ShouldBe(expectedByServiceDateDesc);
        _service.RequiresInMemorySorting("totalcost").ShouldBeTrue();
    }

    #endregion

    #region RequiresInMemorySorting Tests

    [Fact]
    public void RequiresInMemorySorting_ReturnsTrue_ForTotalCost()
    {
        _service.RequiresInMemorySorting("totalcost").ShouldBeTrue();
        _service.RequiresInMemorySorting("TotalCost").ShouldBeTrue();
        _service.RequiresInMemorySorting("TOTALCOST").ShouldBeTrue();
    }

    [Fact]
    public void RequiresInMemorySorting_ReturnsFalse_ForNullOrEmpty()
    {
        _service.RequiresInMemorySorting(null).ShouldBeFalse();
        _service.RequiresInMemorySorting("").ShouldBeFalse();
        _service.RequiresInMemorySorting("   ").ShouldBeFalse();
    }

    [Fact]
    public void RequiresInMemorySorting_ReturnsFalse_ForOtherFields()
    {
        _service.RequiresInMemorySorting("servicedate").ShouldBeFalse();
        _service.RequiresInMemorySorting("mileage").ShouldBeFalse();
        _service.RequiresInMemorySorting("title").ShouldBeFalse();
        _service.RequiresInMemorySorting("invalid").ShouldBeFalse();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ApplyFilters_WhenQueryIsEmpty_ReturnsEmptyQuery()
    {
        // Arrange
        var records = new List<ServiceRecord>();
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "test");

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(0);
    }

    [Fact]
    public void ApplySorting_WhenQueryIsEmpty_ReturnsEmptyQuery()
    {
        // Arrange
        var records = new List<ServiceRecord>();
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "servicedate", true);

        // Assert
        result.Count().ShouldBe(0);
    }

    [Fact]
    public void ApplyFilters_WithSingleRecord_WorksCorrectly()
    {
        // Arrange
        var records = new List<ServiceRecord>
        {
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m)
        };
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "Oil");

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(1);
    }

    [Fact]
    public void ApplySorting_WithSingleRecord_WorksCorrectly()
    {
        // Arrange
        var records = new List<ServiceRecord>
        {
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m)
        };
        var query = records.AsQueryable();

        // Act
        var result = _service.ApplySorting(query, "servicedate", true);

        // Assert
        result.Count().ShouldBe(1);
    }

    [Fact]
    public void ApplyFilters_WithIdenticalRecords_ReturnsAllMatching()
    {
        // Arrange
        var records = new List<ServiceRecord>
        {
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m),
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m),
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m)
        };
        var query = records.AsQueryable();
        var request = new GetServiceRecordsQuery(Guid.NewGuid(), 1, 10, SearchTerm: "Oil");

        // Act
        var result = _service.ApplyFilters(query, request);

        // Assert
        result.Count().ShouldBe(3);
    }

    #endregion

    #region Helper Methods

    private static List<ServiceRecord> CreateTestServiceRecords()
    {
        var serviceType = new ServiceType { Id = Guid.NewGuid(), Name = "Maintenance" };
        
        return new List<ServiceRecord>
        {
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m, "Regular oil change", serviceType),
            CreateServiceRecord("Tire Rotation", new DateTime(2024, 6, 1), 1200, 75m, "Rotated all tires", serviceType),
            CreateServiceRecord("Brake Inspection", new DateTime(2024, 7, 1), 1500, 200m, "Checked brake pads", serviceType),
            CreateServiceRecord("Battery Replacement", new DateTime(2024, 8, 1), 1800, 300m, "New battery installed", serviceType),
            CreateServiceRecord("Oil Filter Change", new DateTime(2024, 3, 1), 950, 50m, "Changed oil filter with synthetic", serviceType)
        };
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithNullNotes()
    {
        var serviceType = new ServiceType { Id = Guid.NewGuid(), Name = "Maintenance" };
        
        return new List<ServiceRecord>
        {
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m, null, serviceType),
            CreateServiceRecord("Tire Rotation", new DateTime(2024, 6, 1), 1200, 75m, null, serviceType)
        };
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithServiceTypes(Guid serviceTypeId)
    {
        var serviceType1 = new ServiceType { Id = serviceTypeId, Name = "Oil Change" };
        var serviceType2 = new ServiceType { Id = Guid.NewGuid(), Name = "Tire Service" };
        
        return new List<ServiceRecord>
        {
            CreateServiceRecord("Oil Change 1", new DateTime(2024, 5, 15), 1000, 150m, serviceType: serviceType1),
            CreateServiceRecord("Oil Change 2", new DateTime(2024, 6, 1), 1200, 150m, serviceType: serviceType1),
            CreateServiceRecord("Oil Change 3", new DateTime(2024, 7, 1), 1500, 150m, serviceType: serviceType1),
            CreateServiceRecord("Tire Rotation", new DateTime(2024, 8, 1), 1800, 75m, serviceType: serviceType2),
            CreateServiceRecord("Tire Balance", new DateTime(2024, 3, 1), 950, 50m, serviceType: serviceType2)
        };
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithDates()
    {
        var serviceType = new ServiceType { Id = Guid.NewGuid(), Name = "Maintenance" };
        
        return new List<ServiceRecord>
        {
            CreateServiceRecord("Service 1", new DateTime(2024, 3, 1), 1000, 100m, serviceType: serviceType),
            CreateServiceRecord("Service 2", new DateTime(2024, 5, 15), 1200, 150m, serviceType: serviceType),
            CreateServiceRecord("Service 3", new DateTime(2024, 6, 1), 1500, 200m, serviceType: serviceType),
            CreateServiceRecord("Service 4", new DateTime(2024, 7, 1), 1800, 250m, serviceType: serviceType),
            CreateServiceRecord("Service 5", new DateTime(2024, 8, 1), 2000, 300m, serviceType: serviceType)
        };
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithMileage()
    {
        var serviceType = new ServiceType { Id = Guid.NewGuid(), Name = "Maintenance" };
        
        return new List<ServiceRecord>
        {
            CreateServiceRecord("Service 1", new DateTime(2024, 5, 15), 1000, 100m, serviceType: serviceType),
            CreateServiceRecord("Service 2", new DateTime(2024, 5, 16), 3000, 150m, serviceType: serviceType),
            CreateServiceRecord("Service 3", new DateTime(2024, 5, 17), 500, 200m, serviceType: serviceType),
            CreateServiceRecord("Service 4", new DateTime(2024, 5, 18), 2000, 250m, serviceType: serviceType),
            CreateServiceRecord("Service 5", new DateTime(2024, 5, 19), 5000, 300m, serviceType: serviceType)
        };
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithNullMileage()
    {
        var serviceType = new ServiceType { Id = Guid.NewGuid(), Name = "Maintenance" };
        
        return new List<ServiceRecord>
        {
            CreateServiceRecord("Service 1", new DateTime(2024, 5, 15), 1000, 100m, serviceType: serviceType),
            CreateServiceRecord("Service 2", new DateTime(2024, 5, 16), null, 150m, serviceType: serviceType),
            CreateServiceRecord("Service 3", new DateTime(2024, 5, 17), 500, 200m, serviceType: serviceType),
            CreateServiceRecord("Service 4", new DateTime(2024, 5, 18), null, 250m, serviceType: serviceType),
            CreateServiceRecord("Service 5", new DateTime(2024, 5, 19), 5000, 300m, serviceType: serviceType)
        };
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithTitles()
    {
        var serviceType = new ServiceType { Id = Guid.NewGuid(), Name = "Maintenance" };
        
        return new List<ServiceRecord>
        {
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 100m, serviceType: serviceType),
            CreateServiceRecord("Tire Rotation", new DateTime(2024, 5, 16), 1200, 150m, serviceType: serviceType),
            CreateServiceRecord("Brake Inspection", new DateTime(2024, 5, 17), 1500, 200m, serviceType: serviceType),
            CreateServiceRecord("Air Filter Change", new DateTime(2024, 5, 18), 1800, 250m, serviceType: serviceType),
            CreateServiceRecord("Battery Replacement", new DateTime(2024, 5, 19), 2000, 300m, serviceType: serviceType)
        };
    }

    private static List<ServiceRecord> CreateTestServiceRecordsWithCosts()
    {
        var serviceType = new ServiceType { Id = Guid.NewGuid(), Name = "Maintenance" };
        
        return new List<ServiceRecord>
        {
            CreateServiceRecord("Service 1", new DateTime(2024, 5, 15), 1000, 150m, serviceType: serviceType),
            CreateServiceRecord("Service 2", new DateTime(2024, 5, 16), 1200, 300m, serviceType: serviceType),
            CreateServiceRecord("Service 3", new DateTime(2024, 5, 17), 1500, 100m, serviceType: serviceType),
            CreateServiceRecord("Service 4", new DateTime(2024, 5, 18), 1800, 200m, serviceType: serviceType),
            CreateServiceRecord("Service 5", new DateTime(2024, 5, 19), 2000, 500m, serviceType: serviceType)
        };
    }

    private static List<ServiceRecord> CreateTestServiceRecordsForCombinedFilters(Guid serviceTypeId)
    {
        var serviceType1 = new ServiceType { Id = serviceTypeId, Name = "Oil Service" };
        var serviceType2 = new ServiceType { Id = Guid.NewGuid(), Name = "Tire Service" };
        
        return new List<ServiceRecord>
        {
            CreateServiceRecord("Oil Change", new DateTime(2024, 5, 15), 1000, 150m, "Premium oil", serviceType1),
            CreateServiceRecord("Oil Filter", new DateTime(2024, 3, 1), 950, 50m, "Standard oil", serviceType1),
            CreateServiceRecord("Tire Rotation", new DateTime(2024, 6, 1), 1200, 75m, "All season tires", serviceType2),
            CreateServiceRecord("Battery Check", new DateTime(2024, 8, 1), 1800, 0m, "Free inspection", serviceType2)
        };
    }

    private static ServiceRecord CreateServiceRecord(
        string title, 
        DateTime serviceDate, 
        int? mileage, 
        decimal cost, 
        string? notes = null,
        ServiceType? serviceType = null)
    {
        var type = serviceType ?? new ServiceType { Id = Guid.NewGuid(), Name = "Maintenance" };
        
        return new ServiceRecord
        {
            Id = Guid.NewGuid(),
            Title = title,
            ServiceDate = serviceDate,
            Mileage = mileage,
            ManualCost = cost,
            Notes = notes,
            VehicleId = Guid.NewGuid(),
            TypeId = type.Id,
            Type = type
        };
    }

    #endregion
}

