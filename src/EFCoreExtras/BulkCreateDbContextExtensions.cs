using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCoreExtras;

public static class BulkCreateDbContextExtensions
{
    /// <summary>
    /// Split (tracked or untracked) objects into batches and build and execute bulk insert query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="objects">Tracked or untracked objects.</param>
    /// <param name="batchSize">Number of objects included for each query.</param>
    /// <returns>Number of written rows.</returns>
    public static async Task<int> BulkCreateAsync<T>(this DbContext context, List<T> objects, int batchSize = 100)
        where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);
        
        int affectedRows = 0;

        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        var queryBuilder = GetSqlBulkQueryBuilder(context);

        foreach (var batch in batches)
        {
            var result = queryBuilder.CreateBulkInsertQuery(context, batch);
            affectedRows += await context.Database.ExecuteSqlRawAsync(result.Query, result.Parameters);
        }

        return affectedRows;
    }

    /// <summary>
    /// Split (tracked or untracked) objects into batches and build and execute bulk insert query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="objects">Tracked or untracked objects.</param>
    /// <param name="batchSize">Number of objects included for each query.</param>
    /// <returns>Number of written rows.</returns>
    public static int BulkCreate<T>(this DbContext context, List<T> objects, int batchSize = 100)
        where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);
        
        int affectedRows = 0;
        
        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        var queryBuilder = GetSqlBulkQueryBuilder(context);

        foreach (var batch in batches)
        {
            var result = queryBuilder.CreateBulkInsertQuery(context, batch);
            affectedRows += context.Database.ExecuteSqlRaw(result.Query, result.Parameters);
        }

        return affectedRows;
    }

    public static ISqlQueryBuilder GetSqlBulkQueryBuilder(this DbContext context)
    {
        var provider = context.GetService<QueryBuilderProvider>();

        var queryBuilder = provider.GetQueryBuilder(context.Database.ProviderName!);

        if (queryBuilder is null)
        {
            throw new InvalidOperationException($"Not supported database provider: {context.Database.ProviderName}");
        }

        return queryBuilder;
    }
}
