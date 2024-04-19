using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Tests;

[TestClass]
public class BulkCreateTest
{
    readonly List<Item> items = [];
    TestDbContext _dbContext = null!;

    [TestInitialize]
    public void Setup()
    {
        items.AddRange([
            new Item { Id = 1, Name = "A", },
            new Item { Id = 2, Name = "B", },
            new Item { Id = 3, Name = "C", },
            new Item { Id = 4, Name = "D", },
            new Item { Id = 5, Name = "E", },
            new Item { Id = 6, Name = "F", },
            new Item { Id = 7, Name = "G", },
            new Item { Id = 8, Name = "H", },
            new Item { Id = 9, Name = "I", },
        ]);
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("Data Source=:memory:") // Using an in-memory database for testing
            .Options;

        _dbContext = new TestDbContext(options);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task BulkCreateT()
    {
        await _dbContext.BulkCreateAsync(items);
    }
}
