using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace EFCoreExtras;

public static class BulkUpdateDbContextExtensions
{
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
    public static async Task<int> BulkUpdateAsync<T>(this DbContext context, List<T> objects, string[] properties, int batchSize = 100) where T : class
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        int affectedRows = 0;

        var batches = ModelSelection.SplitIntoBatches(objects, batchSize);

        var bulkService = context.GetBulkOperationService();

        foreach (var batch in batches)
        {
            var result = bulkService.CreateBulkUpdateQuery(context, objects, properties);
            
            if (result.Ids.Any())
            {
                affectedRows += await context.Database.ExecuteSqlRawAsync(result.Query, result.Parameters);
            }
        }

        context.AttachRange(objects);

        return affectedRows;
    }

    public static Task<int> BulkUpdateAsync<T>(this DbContext context, List<T> objects, Expression<Func<T, object>>[] expressions, int batchSize = 100)
        where T : class
    {
        return BulkUpdateAsync(context, objects, GetPropertyNames(expressions), batchSize);
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
        var bulkService = context.GetBulkOperationService();

        foreach (var batch in batches)
        {
            var result = bulkService.CreateBulkUpdateQuery(context, batch, properties);

            if (result.Ids.Any())
            {
                affectedRows += context.Database.ExecuteSqlRaw(result.Query, result.Parameters);
            }
        }

        context.AttachRange(objects);

        return affectedRows;
    }

    public static int BulkUpdate<T>(this DbContext context, List<T> objects, Expression<Func<T, object>>[] expressions, int batchSize = 100)
        where T : class
    {
        return BulkUpdate(context, objects, GetPropertyNames(expressions), batchSize);
    }

    private static string[] GetPropertyNames<T>(Expression<Func<T, object>>[] expressions)
    {
        List<string> properties = [];

        foreach (var expression in expressions)
        {
            var nodeType = expression.Body.NodeType;

            if (nodeType is ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)expression.Body;
                properties.Add(memberExpression.Member.Name);
            }
            else if (nodeType == ExpressionType.Convert)
            {
                var unexp = (UnaryExpression)expression.Body;
                var opr = (MemberExpression)unexp.Operand;
                properties.Add(opr.Member.Name);
            }
            else
            {
                throw new ArgumentException($"Invalid expression: {expression.Body}");
            }
        }

        return properties.ToArray();
    }
}
