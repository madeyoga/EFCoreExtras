using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreExtras.Tests;

[TestClass]
public class MySql_BulkCreateTest_StringId
{
    readonly List<ItemString> items = [];

    TestDbContext _dbContext = null!;
    IServiceProvider services = null!;
    IServiceScope scope = null!;

    [TestInitialize]
    public void Setup()
    {
        // Add more data
        items.AddRange([
            new ItemString { Name = "A" },
            new ItemString { Name = "B" },
            new ItemString { Name = "C" },
            new ItemString { Name = "D" },
            new ItemString { Name = "E" },
            new ItemString { Name = "F" },
            new ItemString { Name = "G" },
            new ItemString { Name = "H" },
            new ItemString { Name = "I" }
        ]);

        var connectionString = $"server=localhost;user=root;password=;database=efce_test_{Guid.NewGuid()}";
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddEfCoreExtras();
        serviceCollection.AddDbContext<TestDbContext>(o =>
        {
            o.UseMySql(connectionString, serverVersion);
        });

        services = serviceCollection.BuildServiceProvider();
        scope = services.CreateScope();

        _dbContext = scope.ServiceProvider.GetService<TestDbContext>()!;
        _dbContext.Database.EnsureCreated();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();

        scope.Dispose();
    }

    [TestMethod]
    public async Task BulkCreateAsync_ListOfItems_With_StringId()
    {
        await _dbContext.BulkCreateAsync(items, 5);

        Assert.AreEqual(items.Count, _dbContext.ItemStrings.Count());

        var itemExists = true;
        foreach (var item in items)
        {
            itemExists = itemExists && _dbContext.ItemStrings
                .Where(i => i.Id == item.Id)
                .Where(i => i.Name == item.Name)
                .Where(i => i.Quantity == item.Quantity)
                .Any();
        }
        Assert.IsTrue(itemExists);
    }

    [TestMethod]
    public void BulkCreate_ListOfItems_With_StringId()
    {
        _dbContext.BulkCreate(items, 5);

        Assert.AreEqual(items.Count, _dbContext.ItemStrings.Count());

        var itemExists = true;
        foreach (var item in items)
        {
            itemExists = itemExists && _dbContext.ItemStrings
                .Where(i => i.Id == item.Id)
                .Where(i => i.Name == item.Name)
                .Where(i => i.Quantity == item.Quantity)
                .Any();
        }
        Assert.IsTrue(itemExists);
    }
}
