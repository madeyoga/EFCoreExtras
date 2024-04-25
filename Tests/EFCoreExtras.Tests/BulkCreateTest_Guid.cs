using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreExtras.Tests;

[TestClass]
public class BulkCreateTest_Guid
{
    readonly List<Invoice> items = [];

    TestDbContext _dbContext = null!;
    IServiceProvider services = null!;
    IServiceScope scope = null!;

    [TestInitialize]
    public void Setup()
    {
        // Add more data
        items.AddRange([
            new Invoice(),
            new Invoice(),
            new Invoice(),
            new Invoice(),
            new Invoice(),
            new Invoice(),
            new Invoice(),
            new Invoice(),
            new Invoice(),
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
    public async Task BulkCreateAsync_ListOfItems_With_Guid()
    {
        await _dbContext.BulkCreateAsync(items);

        Assert.AreEqual(items.Count, _dbContext.Invoices.Count());

        var itemExists = true;
        foreach (var item in items)
        {
            itemExists = itemExists && _dbContext.Invoices
                .Where(i => i.Id == item.Id)
                .Where(i => i.CreatedAt == item.CreatedAt)
                .Any();
        }
        Assert.IsTrue(itemExists);
    }

    [TestMethod]
    public void BulkCreate_ListOfItems_With_Guid()
    {
        _dbContext.BulkCreate(items);

        Assert.AreEqual(items.Count, _dbContext.Invoices.Count());

        var itemExists = true;
        foreach (var item in items)
        {
            itemExists = itemExists && _dbContext.Invoices
                .Where(i => i.Id == item.Id)
                .Where(i => i.CreatedAt == item.CreatedAt)
                .Any();
        }
        Assert.IsTrue(itemExists);
    }
}
