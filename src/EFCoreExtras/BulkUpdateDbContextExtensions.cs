using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;

namespace EFCoreExtras;

public static class BulkUpdateDbContextExtensions
{
    private static List<List<T>> SplitIntoBatches<T>(List<T> objects, int batchSize)
    {
        var batches = new List<List<T>>();
        for (int i = 0; i < objects.Count; i += batchSize)
        {
            List<T> batch = objects.Skip(i).Take(batchSize).ToList();
            batches.Add(batch);
        }
        return batches;
    }

    public static async Task<int> BulkUpdateWithBatchAsync<T>(this DbContext context, List<T> objects, string[] properties, int batchSize = 100) where T : class
    {
        int affectedRows = 0;

        var batches = SplitIntoBatches(objects, batchSize);

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

        var modelType = typeof(T);
        var tableName = context.Model.FindEntityType(modelType)!.GetTableName();
        var primaryKeyPropertyName = context.Model
            .FindEntityType(modelType)!
            .FindPrimaryKey()!
            .Properties
            .Select(x => x.Name)
            .FirstOrDefault()!;

        // Query:
        //UPDATE product 
        //SET name = CASE
        //      WHEN id = 1 THEN 'New Name 1'
        //      WHEN id = 2 THEN 'New Name 2'...
        //      ELSE name
        //    END,
        //    quantity = CASE
        //      WHEN id = 1 THEN 10
        //      WHEN id = 2 THEN 20...
        //      ELSE quantity
        //    END
        //WHERE id IN(1, 2, ...);

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
                if (!entry.Properties.Where(p => p.IsModified && p.Metadata.Name == fieldName).Any())
                {
                    continue;
                }

                var pkValue = pkProp.GetValue(obj, null)!;
                var fieldValue = fieldProp.GetValue(obj, null)!;

                whenQueryBuilder.Append($"WHEN {primaryKeyPropertyName} = {pkValue} THEN {{{paramIndex++}}} ");
                parameters.Add(fieldValue);
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

        var query = queryBuilder.ToString();

        if (ids.Count == 0)
        {
            return Task.FromResult(0);
        }
        
        return context.Database.ExecuteSqlRawAsync(query, parameters);
    }
}
