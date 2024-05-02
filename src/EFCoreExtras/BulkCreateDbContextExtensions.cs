using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCoreExtras;

public static class BulkCreateDbContextExtensions
{
    /// <summary>
    /// Split objects into batches and execute bulk insert query immediately against database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="objects">Tracked or untracked objects.</param>
    /// <param name="batchSize">Number of objects included for each query.</param>
    /// <returns>Number of written rows.</returns>
    public static async Task<int> BulkCreateAsync<T>(this DbContext context, IEnumerable<T> objects, int batchSize = 100)
        where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);
        
        int affectedRows = 0;

        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        var bulkService = GetBulkOperationService(context);
        foreach (var batch in batches)
        {
            affectedRows += await bulkService.ExecuteBulkInsertAsync(context, batch);
        }

        return affectedRows;
    }

    /// <summary>
    /// Split objects into batches and execute bulk insert query immediately against database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="objects">Tracked or untracked objects.</param>
    /// <param name="batchSize">Number of objects included for each query.</param>
    /// <returns>Number of written rows.</returns>
    public static int BulkCreate<T>(this DbContext context, IEnumerable<T> objects, int batchSize = 100)
        where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);
        
        int affectedRows = 0;
        
        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        var bulkService = GetBulkOperationService(context);
        foreach (var batch in batches)
        {
            affectedRows += bulkService.ExecuteBulkInsert(context, batch);
        }

        return affectedRows;
    }

    public static T[] BulkCreateRetrieve<T>(this DbContext context, IEnumerable<T> objects, int batchSize = 100)
        where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);

        var bulkService = GetBulkOperationService(context);
        return bulkService.ExecuteBulkInsertRetrieve(context, objects, batchSize);
    }

    public static Task<T[]> BulkCreateRetrieveAsync<T>(this DbContext context, IEnumerable<T> objects, int batchSize = 100)
        where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(batchSize, 0);

        var bulkService = GetBulkOperationService(context);
        return bulkService.ExecuteBulkInsertRetrieveAsync(context, objects, batchSize);
    }

    internal static IBulkOperationService GetBulkOperationService(this DbContext context)
    {
        var provider = context.GetService<BulkOperationProvider>();

        var queryBuilder = provider.GetBulkOperationService(context.Database.ProviderName!);

        if (queryBuilder is null)
        {
            throw new InvalidOperationException($"Not supported database provider: {context.Database.ProviderName}");
        }

        return queryBuilder;
    }
}
