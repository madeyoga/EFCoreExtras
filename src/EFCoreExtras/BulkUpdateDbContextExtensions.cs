using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras;

public static class BulkUpdateDbContextExtensions
{
    public static async Task<int> BulkUpdateAsync<T>(this DbContext context, List<T> objects, string[] properties, int batchSize = 100) where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        int affectedRows = 0;

        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        var queryBuilder = context.GetSqlBulkQueryBuilder();

        foreach (var batch in batches)
        {
            var result = queryBuilder.CreateBulkUpdateQuery(context, objects, properties);
            
            if (result.Ids.Count > 0)
            {
                affectedRows += await context.Database.ExecuteSqlRawAsync(result.Query, result.Parameters);
            }
        }

        context.AttachRange(objects);

        return affectedRows;
    }

    /// <summary>
    /// Bulk update given tracked objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="objects">Tracked objects.</param>
    /// <param name="properties"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static int BulkUpdate<T>(this DbContext context, List<T> objects, string[] properties, int batchSize = 100) 
        where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        if (objects.Count == 0 || properties.Length == 0)
            throw new ArgumentException("The objects or properties provided cannot be empty.");

        int affectedRows = 0;

        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);
        var queryBuilder = context.GetSqlBulkQueryBuilder();

        foreach (var batch in batches)
        {
            var result = queryBuilder.CreateBulkUpdateQuery(context, batch, properties);

            if (result.Ids.Count > 0)
            {
                affectedRows += context.Database.ExecuteSqlRaw(result.Query, result.Parameters);
            }
        }

        context.AttachRange(objects);

        return affectedRows;
    }
}
