using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;

namespace EFCoreExtras;

public static class BulkUpdateDbContextExtensions
{
    public static async Task<int> BulkUpdateAsync<T>(this DbContext context, List<T> objects, string[] properties, int batchSize = 100) where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        int affectedRows = 0;

        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        foreach (var batch in batches)
        {
            affectedRows += await BulkUpdateAsync(context, batch, properties);
        }

        return affectedRows;
    }

    public static Task<int> BulkUpdateAsync<T>(this DbContext context, List<T> objects, string[] properties) where T : class
    {
        if (objects.Count == 0 || properties.Length == 0)
            throw new ArgumentException("The objects or properties provided cannot be empty.");

        var result = CreateBulkUpdateQuery(context, objects, properties);

        if (result.Ids.Count == 0)
        {
            return Task.FromResult(0);
        }

        return context.Database.ExecuteSqlRawAsync(result.Query, result.Parameters);
    }

    /// <summary>
    /// Bulk update tracked given objects.
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

        foreach (var batch in batches)
        {
            var result = CreateBulkUpdateQuery(context, batch, properties);

            if (result.Ids.Count > 0)
            {
                affectedRows += context.Database.ExecuteSqlRaw(result.Query, result.Parameters);
            }
        }

        return affectedRows;
    }

    public static CreateBulkUpdateQueryResult CreateBulkUpdateQuery<T>(this DbContext context, List<T> objects, string[] properties)
        where T : class
    {
        var modelType = typeof(T);
        var entityType = context.Model.FindEntityType(modelType)!;
        var tableName = entityType.GetTableName();
        var primaryKeyPropertyName = entityType
            .FindPrimaryKey()!
            .Properties
            .Select(x => x.Name)
            .First();

        var queryBuilder = new StringBuilder();
        queryBuilder.Append($"UPDATE {tableName} SET ");

        PropertyInfo pkProp = modelType.GetProperty(primaryKeyPropertyName)!;
        HashSet<string> ids = [];
        List<object> parameters = [];
        int paramIndex = 0;
        for (int i = 0; i < properties.Length; i++)
        {
            var fieldName = properties[i];
            var fieldProp = modelType.GetProperty(fieldName) ?? throw new ArgumentException($"The property '{fieldName}' does not exist in the type '{modelType.FullName}'.");
            var whenQueryBuilder = new StringBuilder();

            foreach (var obj in objects)
            {
                var entry = context.Entry(obj);

                // Skip if given property name is not modified.
                if (!entry.Properties.Where(p => p.Metadata.Name == fieldName && p.IsModified).Any())
                {
                    continue;
                }

                var pkValue = pkProp.GetValue(obj, null)!;

                whenQueryBuilder.Append($"WHEN {primaryKeyPropertyName} = {pkValue} ");

                var fieldValue = fieldProp.GetValue(obj, null);

                if (fieldValue is null)
                {
                    whenQueryBuilder.Append("THEN NULL ");
                }
                else
                {
                    whenQueryBuilder.Append($"THEN {{{paramIndex++}}} ");
                    parameters.Add(fieldValue);
                }

                ids.Add(pkValue.ToString()!);
            }

            string whenQuery = whenQueryBuilder.ToString();

            if (string.IsNullOrEmpty(whenQuery))
            {
                continue;
            }

            queryBuilder.Append($"{fieldName} = CASE {whenQuery} ELSE {fieldName} END");

            if (i < properties.Length - 1)
                queryBuilder.Append(", ");
        }

        queryBuilder.Append($" WHERE {primaryKeyPropertyName} IN ({string.Join(',', ids)})");

        return new CreateBulkUpdateQueryResult(queryBuilder.ToString(), parameters, ids);
    }
}
