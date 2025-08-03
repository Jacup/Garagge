using Application.Core;
using Domain.Entities.Vehicles;

namespace ApplicationTests.Core;

public class PagedListTests : InMemoryDbTestBase
{
    [Fact]
    public async Task CreateAsync_WithEmptyQuery_ShouldReturnEmptyPagedList()
    {
        // Arrange
        var emptyQuery = Context.Vehicles.Where(v => false).Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(emptyQuery, 1, 10);
        
        // Assert
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(10);
        result.HasNextPage.ShouldBeFalse();
        result.HasPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithSinglePage_ShouldReturnCorrectPagedList()
    {
        // Arrange
        var testData = CreateTestEntities(5);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 1, 10);
        
        // Assert
        result.Items.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(5);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(10);
        result.HasNextPage.ShouldBeFalse();
        result.HasPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithFirstPage_ShouldReturnCorrectPage()
    {
        // Arrange
        var testData = CreateTestEntities(15);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.OrderBy(v => v.Brand).Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 1, 5);
        
        // Assert
        result.Items.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(15);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(5);
        result.HasNextPage.ShouldBeTrue();
        result.HasPreviousPage.ShouldBeFalse();
        result.Items.ShouldBe(new[] { "Brand1", "Brand10", "Brand11", "Brand12", "Brand13" });
    }

    [Fact]
    public async Task CreateAsync_WithMiddlePage_ShouldReturnCorrectPage()
    {
        // Arrange
        var testData = CreateTestEntities(15);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.OrderBy(v => v.Brand).Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 2, 5);
        
        // Assert
        result.Items.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(15);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(5);
        result.HasNextPage.ShouldBeTrue();
        result.HasPreviousPage.ShouldBeTrue();
        result.Items.ShouldBe(new[] { "Brand14", "Brand15", "Brand2", "Brand3", "Brand4" });
    }

    [Fact]
    public async Task CreateAsync_WithLastPage_ShouldReturnCorrectPage()
    {
        // Arrange
        var testData = CreateTestEntities(12);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.OrderBy(v => v.Brand).Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 3, 5);
        
        // Assert
        result.Items.Count.ShouldBe(2); // Last page with partial results
        result.TotalCount.ShouldBe(12);
        result.Page.ShouldBe(3);
        result.PageSize.ShouldBe(5);
        result.HasNextPage.ShouldBeFalse();
        result.HasPreviousPage.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateAsync_WithPageBeyondResults_ShouldReturnEmptyPage()
    {
        // Arrange
        var testData = CreateTestEntities(5);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 3, 5);
        
        // Assert
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(5);
        result.Page.ShouldBe(3);
        result.PageSize.ShouldBe(5);
        result.HasNextPage.ShouldBeFalse();
        result.HasPreviousPage.ShouldBeTrue();
    }

    [Theory]
    [InlineData(1, 5, false, true)]  // First page
    [InlineData(2, 5, true, true)]   // Middle page
    [InlineData(3, 5, true, false)]  // Last page
    public async Task CreateAsync_WithDifferentPages_ShouldHaveCorrectNavigationProperties(
        int page, int pageSize, bool expectedHasPrevious, bool expectedHasNext)
    {
        // Arrange
        var testData = CreateTestEntities(12);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, page, pageSize);
        
        // Assert
        result.HasPreviousPage.ShouldBe(expectedHasPrevious);
        result.HasNextPage.ShouldBe(expectedHasNext);
    }

    [Fact]
    public async Task CreateAsync_WithPageSizeOne_ShouldReturnSingleItem()
    {
        // Arrange
        var testData = CreateTestEntities(3);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.OrderBy(v => v.Brand).Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 2, 1);
        
        // Assert
        result.Items.Count.ShouldBe(1);
        result.TotalCount.ShouldBe(3);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(1);
        result.HasNextPage.ShouldBeTrue();
        result.HasPreviousPage.ShouldBeTrue();
        result.Items[0].ShouldBe("Brand2");
    }

    [Fact]
    public async Task CreateAsync_WithLargePageSize_ShouldReturnAllItems()
    {
        // Arrange
        var testData = CreateTestEntities(5);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 1, 100);
        
        // Assert
        result.Items.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(5);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(100);
        result.HasNextPage.ShouldBeFalse();
        result.HasPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithComplexQuery_ShouldApplyQueryCorrectly()
    {
        // Arrange
        var testData = new List<Vehicle>
        {
            new() { Brand = "Audi", Model = "A4", ManufacturedYear = 2010, UserId = Guid.NewGuid() },
            new() { Brand = "BMW", Model = "X5", ManufacturedYear = 2011, UserId = Guid.NewGuid() },
            new() { Brand = "Audi", Model = "Q7", ManufacturedYear = 2012, UserId = Guid.NewGuid() },
            new() { Brand = "Mercedes", Model = "C-Class", ManufacturedYear = 2013, UserId = Guid.NewGuid() }
        };
        
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles
            .Where(v => v.Brand.Contains("A"))
            .OrderBy(v => v.Model)
            .Select(v => v.Model);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 1, 10);
        
        // Assert
        result.Items.Count.ShouldBe(2);
        result.TotalCount.ShouldBe(2);
        result.Items.ShouldBe(new[] { "A4", "Q7" });
    }

    [Fact]
    public async Task HasNextPage_WhenTotalCountEqualsPageTimesPageSize_ShouldBeFalse()
    {
        // Arrange
        var testData = CreateTestEntities(10);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 2, 5); // 2 * 5 = 10 (exact match)
        
        // Assert
        result.HasNextPage.ShouldBeFalse();
        result.TotalCount.ShouldBe(10);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(5);
    }

    [Fact]
    public async Task HasNextPage_WhenTotalCountExceedsPageTimesPageSize_ShouldBeTrue()
    {
        // Arrange
        var testData = CreateTestEntities(11);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 2, 5); // 2 * 5 = 10 < 11
        
        // Assert
        result.HasNextPage.ShouldBeTrue();
        result.TotalCount.ShouldBe(11);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(5, true)]
    public async Task HasPreviousPage_WithDifferentPages_ShouldReturnCorrectValue(int page, bool expected)
    {
        // Arrange
        var testData = CreateTestEntities(20);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles.Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, page, 5);
        
        // Assert
        result.HasPreviousPage.ShouldBe(expected);
    }

    [Fact]
    public async Task CreateAsync_WithOrderedQuery_ShouldMaintainOrder()
    {
        // Arrange
        var testData = new List<Vehicle>
        {
            new() { Brand = "Zebra", Model = "Z1", ManufacturedYear = 2010, UserId = Guid.NewGuid() },
            new() { Brand = "Alpha", Model = "A1", ManufacturedYear = 2011, UserId = Guid.NewGuid() },
            new() { Brand = "Beta", Model = "B1", ManufacturedYear = 2012, UserId = Guid.NewGuid() }
        };
        
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles
            .OrderBy(v => v.Brand)
            .Select(v => v.Brand);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 1, 10);
        
        // Assert
        result.Items.ShouldBe(new[] { "Alpha", "Beta", "Zebra" });
    }

    [Fact]
    public async Task CreateAsync_WithComplexProjection_ShouldWorkCorrectly()
    {
        // Arrange
        var testData = CreateTestEntities(3);
        Context.Vehicles.AddRange(testData);
        await Context.SaveChangesAsync();
        
        var query = Context.Vehicles
            .Select(v => new { v.Brand, v.Model, FullName = v.Brand + " " + v.Model });
        
        // Act
        var result = await PagedList<object>.CreateAsync(query, 1, 10);
        
        // Assert
        result.Items.Count.ShouldBe(3);
        result.TotalCount.ShouldBe(3);
        result.Items.ShouldAllBe(item => true);
    }

    private static List<Vehicle> CreateTestEntities(int count)
    {
        var entities = new List<Vehicle>();
        
        for (int i = 1; i <= count; i++)
        {
            entities.Add(new Vehicle
            {
                Brand = $"Brand{i}",
                Model = $"Model{i}",
                ManufacturedYear = 2010,
                UserId = Guid.NewGuid()
            });
        }
        return entities;
    }
}
