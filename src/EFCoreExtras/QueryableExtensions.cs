using System.Linq.Expressions;

namespace EFCoreExtras;

public static class QueryableExtensions
{
    /// <summary>
    /// OrderBy column name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertyName"></param>
    /// <param name="method"></param>
    /// <returns><see cref="{IOrderedQueryable[T]}"/></returns>
    public static IOrderedQueryable<T> OrderByColumnUsing<T>(this IQueryable<T> source, string propertyName, string method)
    {
        var parameter = Expression.Parameter(typeof(T), "item");
        var property = Expression.Property(parameter, propertyName);
        var lambda = Expression.Lambda(property, parameter);

        // OrderBy(this IQueryable<T> source, item => item.property)
        var methodCall = Expression.Call(
            typeof(Queryable), 
            method, 
            [parameter.Type, property.Type],
            source.Expression, 
            Expression.Quote(lambda)
        );

        return (IOrderedQueryable<T>) source.Provider.CreateQuery(methodCall);
    }
}
