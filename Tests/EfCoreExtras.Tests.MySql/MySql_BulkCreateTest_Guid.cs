using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Tests;

[TestClass]
public class MySql_BulkCreateTest_Guid
{
    readonly List<Invoice> items = [];

    TestDbContext _dbContext = null!;
    DbContextOptions<TestDbContext> options = null!;

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
