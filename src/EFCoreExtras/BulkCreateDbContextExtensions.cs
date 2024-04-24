using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text;

namespace EFCoreExtras;

public static class BulkCreateDbContextExtensions
{
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
        string primaryKeyPropertyName = entityType.FindPrimaryKey()!
                    .Properties
                    .Select(x => x.Name)
                    .First();

        IEnumerable<IProperty> properties = entityType
            .GetProperties();
            // .Where(p => p.ValueGenerated != ValueGenerated.OnAdd); // Skips autogenerated columns.

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
                if (property.ValueGenerated == ValueGenerated.OnAdd)
                {
                    queryBuilder.Append("NULL, "); // sqlite does not support DEFAULT keyword.
                    continue;
                }

                var propInfo = modelType.GetProperty(property.Name)!;
                var propValue = propInfo.GetValue(obj, null);
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

            queryBuilder.Length -= 2; // Remove ", "
            queryBuilder.Append("), ");
        }

        queryBuilder.Length -= 2; // Remove last 2 characters, a comma anda space.

        return new CreateBulkInsertQueryResult(queryBuilder.ToString(), parameters);
    }
}
