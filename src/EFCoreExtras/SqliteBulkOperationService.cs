﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;
using System.Text;

namespace EFCoreExtras;

public class SqliteBulkOperationService : IBulkOperationService
{
    public BulkInsertQueryResult CreateBulkInsertQuery<T>(DbContext context, List<T> objects)
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

        var primaryKeyPropertyName = entityType
            .FindPrimaryKey()!
            .Properties
            .Select(x => x.Name)
            .First();
        queryBuilder.Append($" RETURNING {primaryKeyPropertyName};");

        return new BulkInsertQueryResult(queryBuilder.ToString(), [..parameters], primaryKeyPropertyName);
    }

    public BulkUpdateQueryResult CreateBulkUpdateQuery<T>(DbContext context, List<T> objects, string[] properties)
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
    
    public int ExecuteBulkInsert<T>(DbContext context, List<T> objects) 
        where T : class
    {
        var result = CreateBulkInsertQuery(context, objects);
        return context.Database.ExecuteSqlRaw(result.Query, result.Parameters);

        //int affectedRows = 0;
        //var entityType = typeof(T);
        //var pkProp = entityType.GetProperty(result.PrimaryKeyPropertyName)!;
        //var pkType = pkProp.PropertyType;

        //if (pkType == typeof(int))
        //{
        //    var ids = context.Database.SqlQueryRaw<int>(result.Query, result.Parameters).ToArray();
        //    //SetPkValues(ids, objects, pkProp);
        //    SetPkValues(pkProp, ids, objects);
        //    //affectedRows += ids.Length;
        //}
        //else if (pkType == typeof(long))
        //{
        //    var ids = context.Database.SqlQueryRaw<long>(result.Query, result.Parameters).ToArray();
        //    SetPkValues(pkProp, ids, objects);
        //    affectedRows += ids.Length;
        //}
        //else if (pkType == typeof(string))
        //{
        //    var ids = context.Database.SqlQueryRaw<string>(result.Query, result.Parameters).ToArray();
        //    SetPkValues(pkProp, ids, objects);
        //    affectedRows += ids.Length;
        //}
        //else if (pkType == typeof(Guid))
        //{
        //    var ids = context.Database.SqlQueryRaw<Guid>(result.Query, result.Parameters).ToArray();
        //    SetPkValues(pkProp, ids, objects);
        //    affectedRows += ids.Length;
        //}
        //else
        //{
        //    // pkType not supported.
        //}
        //return affectedRows;

        //var method = typeof(RelationalDatabaseFacadeExtensions)
        //    .GetMethod("SqlQueryRaw", BindingFlags.Static | BindingFlags.Public)!
        //    .MakeGenericMethod(pkType);
        //var dbResult = method.Invoke(context.Database, [context.Database, result.Query, result.Parameters.ToArray()])!;
        //var toArrayMethod = typeof(Enumerable)
        //    .GetMethod("ToArray", BindingFlags.Public | BindingFlags.Static)!
        //    .MakeGenericMethod(pkType);
        //var ids = toArrayMethod.Invoke(null, [dbResult]);
        //var setPkValuesMethod = typeof(BulkCreateDbContextExtensions)
        //    .GetMethod("SetPkValues", BindingFlags.Static | BindingFlags.NonPublic)!
        //    .MakeGenericMethod(typeof(T), pkType);
        //setPkValuesMethod.Invoke(null, [pkProp, ids, batch]);
    }

    public Task<int> ExecuteBulkInsertAsync<T>(DbContext context, List<T> objects) where T : class
    {
        var result = CreateBulkInsertQuery(context, objects);
        return context.Database.ExecuteSqlRawAsync(result.Query, result.Parameters);
    }

    //private static void SetPkValues<TEntity, TKey>(PropertyInfo pkProp, TKey[] ids, List<TEntity> objects)
    //    where TEntity : class
    //{
    //    var len = ids.Length;
    //    for (int i = 0; i < len; i++)
    //    {
    //        pkProp.SetValue(objects[i], ids[i]);
    //    }
    //}

    //private static readonly Dictionary<string, object> setters = [];

    //private static void SetPkValues<TEntity, TKey>(TKey[] ids, List<TEntity> objects, PropertyInfo propertyInfo)
    //    where TEntity : class
    //{
    //    var len = ids.Length;

    //    Action<TEntity, TKey> setterDelegate;
    //    if (setters.TryGetValue(propertyInfo.Name, out var value))
    //    {
    //        setterDelegate = (Action<TEntity, TKey>) value;
    //    }
    //    else
    //    {
    //        setterDelegate = CreateSetter<TEntity, TKey>(propertyInfo);
    //        setters.Add(propertyInfo.Name, setterDelegate);
    //    }
    //    for (int i = 0; i < len; i++)
    //    {
    //        setterDelegate(objects[i], ids[i]!);
    //    }
    //}

    //public static Action<TEntity, TKey> CreateSetter<TEntity, TKey>(PropertyInfo propertyInfo)
    //{
    //    // delegate(instance, propertyValue)
    //    ParameterExpression instance = Expression.Parameter(typeof(TEntity), "instance");
    //    ParameterExpression propertyValue = Expression.Parameter(typeof(TKey), "propertyValue");

    //    // instance.PropertyName = propertyValue
    //    var body = Expression.Assign(Expression.Property(instance, propertyInfo.Name), propertyValue);

    //    // Action (instance, propertValue) => instance.PropertyName = propertyValue
    //    return Expression.Lambda<Action<TEntity, TKey>>(body, instance, propertyValue).Compile();
    //}
}
