using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras;

public class PaginatedItems<TEntity>(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data)
    where TEntity : class
{
    /// <summary>
    /// Current page index. Page index starts from 1.
    /// </summary>
    public int PageIndex { get; } = pageIndex;

    /// <summary>
    /// Maximum number of items in a single page.
    /// </summary>
    public int PageSize { get; } = pageSize;

    /// <summary>
    /// Total number of items.
    /// </summary>
    public long TotalItems { get; } = count;

    /// <summary>
    /// Total available pages.
    /// </summary>
    public int TotalPages { get; } = (int)Math.Ceiling(count / (double)pageSize);

    public bool HasNextPage { get; } = pageIndex < (int)Math.Ceiling(count / (double)pageSize);

    /// <summary>
    /// Current page items.
    /// </summary>
    public IEnumerable<TEntity> Data { get; } = data;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pageIndex">Page index starts from 1</param>
    /// <param name="pageSize"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task<PaginatedItems<TEntity>> CreateAsync(int pageIndex, int pageSize, IQueryable<TEntity> query)
    {
        var totalItems = await query.LongCountAsync();
        var normalizedIndex = pageIndex - 1;

        query = query.AsNoTracking();

        if (pageSize >= 0)
        {
            query = query
                .Skip(pageSize * normalizedIndex)
                .Take(pageSize);
        }

        var dataInPage = await query.ToListAsync();

        return new PaginatedItems<TEntity>(pageIndex, pageSize, totalItems, dataInPage);
    }
}

public static class Pagination
{
    public static Task<PaginatedItems<TEntity>> CreateAsync<TEntity>(int pageIndex, int pageSize, IQueryable<TEntity> query)
        where TEntity : class
    {
        return PaginatedItems<TEntity>.CreateAsync(pageIndex, pageSize, query);
    }
}
