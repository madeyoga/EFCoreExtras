namespace EFCoreExtras.Tests;

[TestClass]
public class PaginationTest
{
    [TestMethod]
    public async Task CreatePaginatedItems()
    {
        List<Item> items = [
            new Item { Id = 1, Name = "A", Quantity = 10 },
            new Item { Id = 2, Name = "B", Quantity = 10 },
            new Item { Id = 3, Name = "C", Quantity = 10 },
            new Item { Id = 4, Name = "D", Quantity = 10 },
            new Item { Id = 5, Name = "E", Quantity = 10 },
            new Item { Id = 6, Name = "F", Quantity = 10 },
            new Item { Id = 7, Name = "G", Quantity = 10 },
            new Item { Id = 8, Name = "H", Quantity = 10 },
            new Item { Id = 9, Name = "I", Quantity = 10 },
        ];

        var paginatedItems = await Pagination.CreateAsync(1, 5, items.AsQueryable());

        Assert.Equals(paginatedItems.PageIndex, 1);
        Assert.Equals(paginatedItems.PageSize, 5);
        Assert.Equals(paginatedItems.TotalPages, 2);
        Assert.Equals(paginatedItems.TotalItems, items.Count);
        Assert.Equals(paginatedItems.Data.Count(), 5);
        Assert.IsTrue(paginatedItems.HasNextPage);

        var paginatedItems2 = await Pagination.CreateAsync(2, 5, items.AsQueryable());

        Assert.Equals(paginatedItems2.PageIndex, 2);
        Assert.Equals(paginatedItems2.PageSize, 5);
        Assert.Equals(paginatedItems2.TotalPages, 2);
        Assert.Equals(paginatedItems2.TotalItems, items.Count);
        Assert.Equals(paginatedItems2.Data.Count(), 4);
        Assert.IsFalse(paginatedItems.HasNextPage);
    }
}
