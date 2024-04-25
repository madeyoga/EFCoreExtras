using Microsoft.EntityFrameworkCore;
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

        var result = CreateBulkInsertQuery(context, objects);

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

        var result = CreateBulkInsertQuery(context, objects);

        return context.Database.ExecuteSqlRaw(result.Query, result.Parameters);
    }

    public static CreateBulkInsertQueryResult CreateBulkInsertQuery<T>(this DbContext context, List<T> objects)
        where T : class
    {
        if (objects.Count == 0)
        {
            throw new ArgumentException("The objects provided cannot be empty.");
        }

        var modelType = typeof(T);
        IEntityType entityType = context.Model.FindEntityType(modelType)!;
        string tableName = entityType.GetTableName()!;

        IEnumerable<IProperty> properties = entityType.GetDeclaredProperties();

        var propertyNames = properties
            .Select(p => p.GetColumnName())
            .ToList();

        var queryBuilder = new StringBuilder();
        queryBuilder.Append($"INSERT INTO {tableName} ({string.Join(',', propertyNames)}) VALUES ");

        List<object> parameters = [];
        int paramIndex = 0;
        foreach (var obj in objects)
        {
            queryBuilder.Append('(');

            foreach (var property in properties)
            {
                var propInfo = modelType.GetProperty(property.Name)!;
                var propValue = propInfo.GetValue(obj, null);

                if (property.ValueGenerated == ValueGenerated.OnAdd)
                {
                    // Auto generated property is not set to any value.
                    if (propValue is null || propValue.Equals(0))
                    {
                        queryBuilder.Append("NULL, "); // sqlite does not support DEFAULT keyword.
                    }
                    else
                    {
                        parameters.Add(propValue);
                        queryBuilder.Append($"{{{paramIndex++}}}, ");
                    }
                }
                else
                {
                    if (propValue is null)
                    {
                        queryBuilder.Append("NULL, ");
                    }
                    else
                    {
                        parameters.Add(propValue);
                        queryBuilder.Append($"{{{paramIndex++}}}, ");
                    }
                }
            }

            queryBuilder.Length -= 2; // Remove ", "
            queryBuilder.Append("), ");
        }

        queryBuilder.Length -= 2; // Remove last 2 characters, a comma anda space.

        return new CreateBulkInsertQueryResult(queryBuilder.ToString(), parameters);
    }
}
