using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Tests;

[TestClass]
public class MySql_BulkUpdateTests
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
        var connectionString = $"server=localhost;user=root;password=;database=efce_test_{Guid.NewGuid()}";
        var serverVersion = ServerVersion.AutoDetect(connectionString);
        options = new DbContextOptionsBuilder<TestDbContext>()
            .UseMySql(connectionString, serverVersion)
            .Options;

        _dbContext = new TestDbContext(options);
        _dbContext.Database.EnsureCreated();

        _dbContext.Items.AddRange(items);
        _dbContext.SaveChanges();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task BulkUpdateAsyncListOfItems()
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

    [TestMethod]
    public void BulkUpdateListOfItems()
    {
        var items = _dbContext.Items.ToList();

        foreach (var item in items) 
        {
            item.Name = $"{item.Name} {item.Id}";
            item.Quantity += 10;
        }

        _dbContext.BulkUpdate(items, ["Name", "Quantity"]);

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