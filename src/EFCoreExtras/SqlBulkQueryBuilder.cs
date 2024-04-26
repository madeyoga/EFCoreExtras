using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;
using System.Text;

namespace EFCoreExtras;

public class SqlBulkQueryBuilder : ISqlQueryBuilder
{
    public CreateBulkInsertQueryResult CreateBulkInsertQuery<T>(DbContext context, List<T> objects)
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

    public CreateBulkUpdateQueryResult CreateBulkUpdateQuery<T>(DbContext context, List<T> objects, string[] properties)
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

            queryBuilder.Append($"{fieldName} = CASE {whenQuery} ELSE {fieldName} END, ");
        }

        queryBuilder.Length -= 2;

        queryBuilder.Append($" WHERE {primaryKeyPropertyName} IN ({string.Join(',', ids)})");

        return new CreateBulkUpdateQueryResult(queryBuilder.ToString(), parameters, ids);
    }
}
