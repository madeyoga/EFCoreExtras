using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreExtras.Tests;

[TestClass]
public class BulkCreateTest
{
    readonly List<Item> items = [];
    readonly List<Item> itemsAutoIncrement = [];

    TestDbContext _dbContext = null!;
    IServiceProvider services = null!;
    IServiceScope scope = null!;

    [TestInitialize]
    public void Setup()
    {
        // Add more data
        items.AddRange([
            new Item { Id = 11, Name = "A", },
            new Item { Id = 2, Name = "B", },
            new Item { Id = 3, Name = "C", },
            new Item { Id = 4, Name = "D", },
            new Item { Id = 5, Name = "E", },
            new Item { Id = 6, Name = "F", },
            new Item { Id = 7, Name = "G", },
            new Item { Id = 8, Name = "H", },
            new Item { Id = 9, Name = "I", },
        ]);

        // Auto increment Id
        itemsAutoIncrement.AddRange([
            new Item { Name = "A", },
            new Item { Name = "B", },
            new Item { Name = "C", },
            new Item { Name = "D", },
            new Item { Name = "E", },
            new Item { Name = "A", },
            new Item { Name = "B", },
            new Item { Name = "C", },
            new Item { Name = "D", },
            new Item { Name = "E", },
            new Item { Name = "A", },
            new Item { Name = "B", },
            new Item { Name = "C", },
            new Item { Name = "D", },
            new Item { Name = "E", },
            new Item { Name = "A", },
            new Item { Name = "B", },
            new Item { Name = "C", },
            new Item { Name = "D", },
            new Item { Name = "E", },
        ]);

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddEfCoreExtras();
        serviceCollection.AddDbContext<TestDbContext>(o =>
        {
            o.UseSqlite("DataSource=:memory:");
        });

        services = serviceCollection.BuildServiceProvider();
        scope = services.CreateScope();

        _dbContext = scope.ServiceProvider.GetService<TestDbContext>()!;
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();

        scope.Dispose();
    }

    [TestMethod]
    public async Task BulkCreateAsyncListOfItems()
    {
        var writtenRows = await _dbContext.BulkCreateAsync(items, 5);

        Assert.AreEqual(items.Count, _dbContext.Items.Count());
        Assert.AreEqual(items.Count, writtenRows);

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
        var writtenRows = _dbContext.BulkCreate(items, 5);

        Assert.AreEqual(items.Count, _dbContext.Items.Count());
        Assert.AreEqual(items.Count, writtenRows);

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
    public async Task BulkCreateAsyncListOfItems_AutoIncrement()
    {
        var items = await _dbContext.BulkCreateRetrieveAsync(itemsAutoIncrement, 5);

        Assert.AreEqual(itemsAutoIncrement.Count, _dbContext.Items.Count());
        Assert.AreEqual(itemsAutoIncrement.Count, items.Length);

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
    public void BulkCreateListOfItems_AutoIncrement()
    {
        var items = _dbContext.BulkCreateRetrieve(itemsAutoIncrement, 5);

        Assert.AreEqual(itemsAutoIncrement.Count, _dbContext.Items.Count());
        Assert.AreEqual(itemsAutoIncrement.Count, items.Length);

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
