using Application.Core;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTests.Core;

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    
    public DbSet<TestEntity> TestEntities { get; set; } = null!;
}

public class PagedListTests
{
    private DbContextOptions<TestDbContext> GetInMemoryDbOptions()
    {
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateAsync_WithEmptyQuery_ShouldReturnEmptyPagedList()
    {
        // Arrange
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var emptyQuery = context.TestEntities.Where(x => false).Select(x => x.Name);
        
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
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(5);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.Select(x => x.Name);
        
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
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(15);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.OrderBy(x => x.Name).Select(x => x.Name);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 1, 5);
        
        // Assert
        result.Items.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(15);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(5);
        result.HasNextPage.ShouldBeTrue();
        result.HasPreviousPage.ShouldBeFalse();
        result.Items.ShouldBe(new[] { "Item1", "Item10", "Item11", "Item12", "Item13" });
    }

    [Fact]
    public async Task CreateAsync_WithMiddlePage_ShouldReturnCorrectPage()
    {
        // Arrange
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(15);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.OrderBy(x => x.Name).Select(x => x.Name);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 2, 5);
        
        // Assert
        result.Items.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(15);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(5);
        result.HasNextPage.ShouldBeTrue();
        result.HasPreviousPage.ShouldBeTrue();
        result.Items.ShouldBe(new[] { "Item14", "Item15", "Item2", "Item3", "Item4" });
    }

    [Fact]
    public async Task CreateAsync_WithLastPage_ShouldReturnCorrectPage()
    {
        // Arrange
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(12);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.OrderBy(x => x.Name).Select(x => x.Name);
        
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
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(5);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.Select(x => x.Name);
        
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
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(12);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.Select(x => x.Name);
        
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
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(3);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.OrderBy(x => x.Name).Select(x => x.Name);
        
        // Act
        var result = await PagedList<string>.CreateAsync(query, 2, 1);
        
        // Assert
        result.Items.Count.ShouldBe(1);
        result.TotalCount.ShouldBe(3);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(1);
        result.HasNextPage.ShouldBeTrue();
        result.HasPreviousPage.ShouldBeTrue();
        result.Items[0].ShouldBe("Item2");
    }

    [Fact]
    public async Task CreateAsync_WithLargePageSize_ShouldReturnAllItems()
    {
        // Arrange
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(5);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.Select(x => x.Name);
        
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
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = new List<TestEntity>
        {
            new() { Name = "Audi", Value = 1 },
            new() { Name = "BMW", Value = 2 },
            new() { Name = "Audi", Value = 3 },
            new() { Name = "Mercedes", Value = 4 }
        };
        
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities
            .Where(v => v.Name.Contains("A"))
            .OrderBy(v => v.Value)
            .Select(v => v.Value);
        
        // Act
        var result = await PagedList<int>.CreateAsync(query, 1, 10);
        
        // Assert
        result.Items.Count.ShouldBe(2);
        result.TotalCount.ShouldBe(2);
        result.Items.ShouldBe(new[] { 1, 3 });
    }

    [Fact]
    public async Task HasNextPage_WhenTotalCountEqualsPageTimesPageSize_ShouldBeFalse()
    {
        // Arrange
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(10);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.Select(x => x.Name);
        
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
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(11);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.Select(x => x.Name);
        
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
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var testData = CreateTestEntities(20);
        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities.Select(x => x.Value);
        
        // Act
        var result = await PagedList<int>.CreateAsync(query, page, 5);
        
        // Assert
        result.HasPreviousPage.ShouldBe(expected);
    }

    [Fact]
    public async Task CreateAsync_WithEnergyEntryRelatedData_ShouldHandleComplexQueries()
    {
        // Arrange
        await using var context = new TestDbContext(GetInMemoryDbOptions());
        var energyData = new List<TestEntity>();
        
        for (int i = 1; i <= 15; i++)
        {
            energyData.Add(new TestEntity
            {
                Id = i,
                Name = i % 2 == 0 ? "Electric" : "Gasoline",
                Value = i * 50
            });
        }
        
        context.TestEntities.AddRange(energyData);
        await context.SaveChangesAsync();
        
        var query = context.TestEntities
            .Where(e => e.Name == "Electric")
            .OrderBy(e => e.Value)
            .Select(e => e.Value);
        
        // Act
        var result = await PagedList<int>.CreateAsync(query, 2, 3);
        
        // Assert
        result.Items.Count.ShouldBe(3);
        result.TotalCount.ShouldBe(7); // 7 electric entries (even numbers)
        result.HasNextPage.ShouldBeTrue();
        result.HasPreviousPage.ShouldBeTrue();
        result.Page.ShouldBe(2);
    }

    private static List<TestEntity> CreateTestEntities(int count)
    {
        var entities = new List<TestEntity>();
        
        for (int i = 1; i <= count; i++)
        {
            entities.Add(new TestEntity
            {
                Id = i,
                Name = $"Item{i}",
                Value = i * 10
            });
        }
        return entities;
    }
}
