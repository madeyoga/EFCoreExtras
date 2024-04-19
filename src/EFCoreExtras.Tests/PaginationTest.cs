using MockQueryable.Moq;

namespace EFCoreExtras.Tests;

[TestClass]
public class PaginationTest
{
    readonly List<Item> items = [];

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
    }

    [TestMethod]
    public async Task CreatePaginatedItems()
    {
        var query = items.BuildMock();

        var paginatedItems = await Pagination.CreateAsync(1, 5, query);

        Assert.AreEqual(1, paginatedItems.PageIndex);
        Assert.AreEqual(5, paginatedItems.PageSize);
        Assert.AreEqual(2, paginatedItems.TotalPages);
        Assert.AreEqual(items.Count, paginatedItems.TotalItems);
        Assert.AreEqual(5, paginatedItems.Data.Count());

        var paginatedItems2 = await Pagination.CreateAsync(2, 5, query);

        Assert.AreEqual(2, paginatedItems2.PageIndex);
        Assert.AreEqual(5, paginatedItems2.PageSize);
        Assert.AreEqual(2, paginatedItems2.TotalPages);
        Assert.AreEqual(items.Count, paginatedItems2.TotalItems);
        Assert.AreEqual(4, paginatedItems2.Data.Count());
    }

    [TestMethod]
    public async Task NegativePageSize_ReturnsAllObjects()
    {
        var query = items.BuildMock();

        var paginatedItems = await Pagination.CreateAsync(1, -1, query);

        Assert.AreEqual(1, paginatedItems.PageIndex);
        Assert.AreEqual(items.Count, paginatedItems.TotalItems);
        Assert.AreEqual(items.Count, paginatedItems.Data.Count());
    }

    [TestMethod]
    public async Task LastPage_HasNextPage_False()
    {
        var query = items.BuildMock();

        var paginatedItems = await Pagination.CreateAsync(2, 5, query);

        Assert.IsFalse(paginatedItems.HasNextPage);
    }
}
