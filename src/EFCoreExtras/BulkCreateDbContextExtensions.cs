using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text;

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
    /// <returns></returns>
    public static async Task<int> BulkCreateAsync<T>(this DbContext context, List<T> objects, int batchSize = 100)
        where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);
        
        int affectedRows = 0;

        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        foreach (var batch in batches)
        {
            affectedRows += await BulkCreateAsync(context, batch);
        }

        return affectedRows;
    }

    /// <summary>
    /// Build bulk insert query from the given tracked or untracked objects and execute the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="objects">Tracked or untracked objects.</param>
    /// <returns>Number of affected rows.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Task<int> BulkCreateAsync<T>(this DbContext context, List<T> objects)
        where T : class
    {
        if (objects.Count == 0)
        {
            throw new ArgumentException("The objects provided cannot be empty.");
        }

        var queryBuilder = GetSqlBulkQueryBuilder(context);
        var result = queryBuilder.CreateBulkInsertQuery(context, objects);

        return context.Database.ExecuteSqlRawAsync(result.Query, result.Parameters);
    }

    public static int BulkCreate<T>(this DbContext context, List<T> objects, int batchSize = 100)
        where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);
        
        int affectedRows = 0;
        
        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        foreach (var batch in batches)
        {
            affectedRows += BulkCreate(context, batch);
        }

        return affectedRows;
    }

    public static int BulkCreate<T>(this DbContext context, List<T> objects)
        where T : class
    {
        if (objects.Count == 0)
        {
            throw new ArgumentException("The objects provided cannot be empty.");
        }

        var queryBuilder = GetSqlBulkQueryBuilder(context);
        var result = queryBuilder.CreateBulkInsertQuery(context, objects);

        return context.Database.ExecuteSqlRaw(result.Query, result.Parameters);
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
