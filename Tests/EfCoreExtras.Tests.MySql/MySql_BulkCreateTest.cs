using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Tests;

[TestClass]
public class MySql_BulkCreateTest
{
    readonly List<Item> items = [];

    TestDbContext _dbContext = null!;
    DbContextOptions<TestDbContext> options = null!;

    [TestInitialize]
    public void Setup()
    {
        // Add more data
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
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task BulkCreateAsyncListOfItems()
    {
        await _dbContext.BulkCreateAsync(items, 5);

        Assert.AreEqual(items.Count, _dbContext.Items.Count());

        var itemExists = true;
        foreach (var item in items)
        {
            itemExists = itemExists && _dbContext.Items
                .Where(i => i.Id == item.Id)
                .Where(i => i.Name == item.Name)
                .Where(i => i.Quantity == item.Quantity)
                .Any();
        }
        Assert.IsTrue(itemExists);
    }

    [TestMethod]
    public void BulkCreateListOfItems()
    {
        _dbContext.BulkCreate(items, 5);

        Assert.AreEqual(items.Count, _dbContext.Items.Count());

        var itemExists = true;
        foreach (var item in items)
        {
            itemExists = itemExists && _dbContext.Items
                .Where(i => i.Id == item.Id)
                .Where(i => i.Name == item.Name)
                .Where(i => i.Quantity == item.Quantity)
                .Any();
        }
        Assert.IsTrue(itemExists);
    }
}