using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Tests;

[TestClass]
public class BulkCreateTest
{
    readonly List<Item> items = [];

    TestDbContext _dbContext = null!;
    DbContextOptions<TestDbContext> options = null!;

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
        options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:") // Using an in-memory database for testing
            .Options;

        _dbContext = new TestDbContext(options);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task BulkCreateT()
    {
        await _dbContext.BulkCreateAsync(items);

        Assert.AreEqual(items.Count, _dbContext.Items.Count());

        foreach (var item in items)
        {
            var itemExists = _dbContext.Items.Where(i => i.Id == item.Id).Any();
            Assert.IsTrue(itemExists);
        }
    }
}
