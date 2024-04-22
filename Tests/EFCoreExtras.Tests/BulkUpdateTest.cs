using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Tests;

[TestClass]
public class BulkUpdateTests
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

        _dbContext.Items.AddRange(items);
        _dbContext.SaveChanges();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task BulkUpdateListOfItems()
    {
        var items = await _dbContext.Items.ToListAsync();

        foreach (var item in items) 
        {
            item.Name = $"{item.Name} {item.Id}";
            item.Quantity += 10;
        }

        await _dbContext.BulkUpdateAsync(items, ["Name", "Quantity"]);

        var updated = true;

        foreach(var item in items)
        {
            updated = updated && _dbContext.Items
                .Where(i => i.Name == item.Name && i.Quantity == item.Quantity)
                .Any();
        }

        Assert.IsTrue(updated);
    }
}