using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EFCoreExtras;

public class SqliteBulkOperationService : IBulkOperationService
{
    readonly MethodInfo bulkInsertBatches;
    readonly MethodInfo bulkInsertBatchesAsync;

    public SqliteBulkOperationService()
    {
        bulkInsertBatches = typeof(SqliteBulkOperationService)
            .GetMethod("ExecuteBulkInsertBatches", BindingFlags.NonPublic | BindingFlags.Static)!;
        bulkInsertBatchesAsync = typeof(SqliteBulkOperationService)
            .GetMethod("ExecuteBulkInsertBatchesAsync", BindingFlags.NonPublic | BindingFlags.Static)!;
    }


    public static BulkInsertQueryResult CreateBulkInsertQuery<T>(DbContext context, IEnumerable<T> objects, bool retrieveKeys = false)
        where T : class
    {
        if (!objects.Any())
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
                var propInfo = modelType.GetProperty(property.Name)!; // can be cached
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

        if (retrieveKeys)
        {
            var key = entityType.FindPrimaryKey();
            if (key is not null)
            {
                var primaryKeyPropertyName = key.Properties[0].GetColumnName();
                queryBuilder.Append($" RETURNING {primaryKeyPropertyName};"); // Retrieve primary key values.
            }
        }

        return new BulkInsertQueryResult(queryBuilder.ToString(), [..parameters]);
    }

    public static BulkUpdateQueryResult CreateBulkUpdateQuery<T>(DbContext context, IEnumerable<T> objects, string[] properties)
        where T : class
    {
        var modelType = typeof(T);
        var entityType = context.Model.FindEntityType(modelType)!;
        var tableName = entityType.GetTableName();
        var primaryKeyPropertyName = entityType
            .FindPrimaryKey()!
            .Properties[0]
            .GetColumnName();
        var pkProp = modelType.GetProperty(primaryKeyPropertyName)!;

        Dictionary<string, StringBuilder> cache = [];
        List<PropertyInfo> propertyInfos = [];
        foreach (var propName in properties)
        {
            var propInfo = modelType.GetProperty(propName) ?? throw new ArgumentException($"The property '{propName}' does not exist in the type '{modelType.FullName}'.");
            propertyInfos.Add(propInfo);
            cache[propName] = new StringBuilder();
        }

        var queryBuilder = new StringBuilder();
        queryBuilder.Append($"UPDATE {tableName} SET ");
        List<string> ids = [];
        List<object> parameters = [];
        int paramIndex = 0;
        foreach (var obj in objects)
        {
            var entry = context.Entry(obj);

            var pkValue = pkProp.GetValue(obj, null)!;
            ids.Add(pkValue.ToString()!);

            foreach (var prop in propertyInfos)
            {
                var stringBuilder = cache[prop.Name];
                stringBuilder.Append($"WHEN {primaryKeyPropertyName} = {pkValue} ");

                var fieldValue = prop.GetValue(obj, null);

                if (fieldValue is null)
                {
                    stringBuilder.Append("THEN NULL ");
                }
                else
                {
                    stringBuilder.Append($"THEN {{{paramIndex++}}} ");
                    parameters.Add(fieldValue);
                }
            }
        }

        foreach (var key in cache.Keys)
        {
            queryBuilder.Append($"{key} = CASE {cache[key].ToString()} ELSE {key} END, ");
        }

        queryBuilder.Length -= 2;
        queryBuilder.Append($" WHERE {primaryKeyPropertyName} IN ({string.Join(',', ids)})");

        return new BulkUpdateQueryResult(queryBuilder.ToString(), parameters, ids);
    }

    /// <summary>
    /// This operation executes immediately against database. It also does not interact with EF ChangeTracker.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="objects"></param>
    /// <returns></returns>
    public int ExecuteBulkInsert<T>(DbContext context, IEnumerable<T> objects) 
        where T : class
    {
        var result = CreateBulkInsertQuery(context, objects);
        return context.Database.ExecuteSqlRaw(result.Query, result.Parameters);
    }

    /// <summary>
    /// This operation executes immediately against database. It also does not interact with EF ChangeTracker.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <param name="objects"></param>
    /// <returns></returns>
    public Task<int> ExecuteBulkInsertAsync<T>(DbContext context, IEnumerable<T> objects) where T : class
    {
        var result = CreateBulkInsertQuery(context, objects);
        return context.Database.ExecuteSqlRawAsync(result.Query, result.Parameters);
    }

    public static Action<TEntity, TKey> CreateSetter<TEntity, TKey>(string propertyName)
    {
        // delegate(instance, propertyValue)
        ParameterExpression instance = Expression.Parameter(typeof(TEntity), "instance");
        ParameterExpression propertyValue = Expression.Parameter(typeof(TKey), "propertyValue");

        // instance.PropertyName = propertyValue
        var body = Expression.Assign(Expression.Property(instance, propertyName), propertyValue);

        // Action (instance, propertValue) => instance.PropertyName = propertyValue
        return Expression.Lambda<Action<TEntity, TKey>>(body, instance, propertyValue).Compile();
    }

    private static int ExecuteBulkInsertBatches<T, TKey>(DbContext context, T[][] batches, PropertyInfo pkProp)
        where T : class
    {
        int affectedRows = 0;
        foreach (var batch in batches)
        {
            var result = CreateBulkInsertQuery(context, batch, retrieveKeys: true);
            var ids = context.Database.SqlQueryRaw<TKey>(result.Query, result.Parameters).ToArray();
            var len = ids.Length;
            for (var i = 0; i < len; i++)
            {
                //setter(batch[i], ids[i]);
                pkProp.SetValue(batch[i], ids[i]);
            }

            affectedRows += len;
        }

        return affectedRows;
    }

    private static async Task<int> ExecuteBulkInsertBatchesAsync<T, TKey>(DbContext context, T[][] batches, PropertyInfo pkProp)
        where T : class
    {
        int affectedRows = 0;
        foreach (var batch in batches)
        {
            var result = CreateBulkInsertQuery(context, batch, retrieveKeys: true);
            var ids = await context.Database.SqlQueryRaw<TKey>(result.Query, result.Parameters).ToArrayAsync();
            var len = ids.Length;
            for (var i = 0; i < len; i++)
            {
                //setter(batch[i], ids[i]);
                pkProp.SetValue(batch[i], ids[i]);
            }

            affectedRows += len;
        }

        return affectedRows;
    }

    public int ExecuteBulkInsertRetrieveKeys<T>(DbContext context, IEnumerable<T> objects, int batchSize) where T : class
    {
        //var setter = CreateSetter<T, int>("Id");
        var modelType = typeof(T);
        var entityType = context.Model.FindEntityType(modelType)!;
        var pkPropertyName = entityType
            .FindPrimaryKey()!
            .Properties[0]
            .GetColumnName();
        var pkProp = modelType.GetProperty(pkPropertyName)!;
        var pkType = pkProp.PropertyType;

        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        var method = bulkInsertBatches.MakeGenericMethod(typeof(T), pkType);

        return (int) method.Invoke(this, [context, batches, pkProp])!;
    }

    public async Task<int> ExecuteBulkInsertRetrieveKeysAsync<T>(DbContext context, IEnumerable<T> objects, int batchSize = 100) where T : class
    {
        var modelType = typeof(T);
        var entityType = context.Model.FindEntityType(modelType)!;
        var pkPropertyName = entityType
            .FindPrimaryKey()!
            .Properties[0]
            .GetColumnName();
        var pkProp = modelType.GetProperty(pkPropertyName)!;
        var pkType = pkProp.PropertyType;

        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        var method = bulkInsertBatchesAsync.MakeGenericMethod(typeof(T), pkType);

        return await (Task<int>) method.Invoke(this, [context, batches, pkProp])!;
    }

    public int ExecuteBulkUpdate<T>(DbContext context, IEnumerable<T> objects, string[] properties) where T : class
    {
        var result = CreateBulkUpdateQuery(context, objects, properties);
        return context.Database.ExecuteSqlRaw(result.Query, result.Parameters);
    }

    public Task<int> ExecuteBulkUpdateAsync<T>(DbContext context, IEnumerable<T> objects, string[] properties) where T : class
    {
        var result = CreateBulkUpdateQuery(context, objects, properties);
        return context.Database.ExecuteSqlRawAsync(result.Query, result.Parameters);
    }
}
