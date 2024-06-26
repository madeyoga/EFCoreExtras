﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;
using System.Text;

namespace EFCoreExtras;

public class RelationalBulkOperationService : IBulkOperationService
{
    public static BulkInsertQueryResult CreateBulkInsertQuery<T>(DbContext context, IEnumerable<T> objects)
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
            .Properties
            .Select(x => x.Name)
            .First();
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

    public int ExecuteBulkInsert<T>(DbContext context, IEnumerable<T> objects) where T : class
    {
        var result = CreateBulkInsertQuery(context, objects);
        return context.Database.ExecuteSqlRaw(result.Query, result.Parameters);
    }

    public Task<int> ExecuteBulkInsertAsync<T>(DbContext context, IEnumerable<T> objects) where T : class
    {
        var result = CreateBulkInsertQuery(context, objects);
        return context.Database.ExecuteSqlRawAsync(result.Query, result.Parameters);
    }

    public T[] ExecuteBulkInsertRetrieve<T>(DbContext context, IEnumerable<T> objects, int batchSize = 100) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<T[]> ExecuteBulkInsertRetrieveAsync<T>(DbContext context, IEnumerable<T> objects, int batchSize = 100) where T : class
    {
        throw new NotImplementedException();
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
