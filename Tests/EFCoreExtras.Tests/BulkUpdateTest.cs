using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace EFCoreExtras.Tests;

[TestClass]
public class BulkUpdateTests
{
    readonly List<Item> items = [];

    TestDbContext _dbContext = null!;
    IServiceProvider services = null!;
    IServiceScope scope = null!;

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

        scope.Dispose();
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
    public async Task BulkUpdateAsyncListOfItems_Typed()
    {
        var items = _dbContext.Items.ToList();

        foreach (var item in items)
        {
            item.Name = $"{item.Name} {item.Id}";
            item.Quantity += 10;
        }

        await _dbContext.BulkUpdateAsync(items, [
            item => item.Name,
            item => item.Quantity,
        ]);

        var updated = true;

        foreach (var item in items)
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

    [TestMethod]
    public void BulkUpdateListOfItems_Typed()
    {
        var items = _dbContext.Items.ToList();

        foreach (var item in items)
        {
            item.Name = $"{item.Name} {item.Id}";
            item.Quantity += 10;
        }

        _dbContext.BulkUpdate(items, [
            item => item.Name,
            item => item.Quantity,
        ]);

        var updated = true;

        foreach (var item in items)
        {
            updated = updated && _dbContext.Items
                .Where(i => i.Name == item.Name && i.Quantity == item.Quantity)
                .Any();
        }

        Assert.IsTrue(updated);
    }
}