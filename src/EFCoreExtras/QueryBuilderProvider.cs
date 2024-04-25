using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace EFCoreExtras;

public class QueryBuilderProvider
{
    private readonly Dictionary<string, QueryBuilderScheme> _schemes = new(StringComparer.Ordinal);
    private readonly object _lock = new();
    private readonly IServiceProvider _serviceProvider;

    public QueryBuilderProvider(IServiceProvider serviceProvider)
    {
        AddQueryBuilderScheme(new QueryBuilderScheme("Microsoft.EntityFrameworkCore.Sqlite", typeof(SqlBulkQueryBuilder)));
        AddQueryBuilderScheme(new QueryBuilderScheme("Npgsql.EntityFrameworkCore.PostgreSQL", typeof(SqlBulkQueryBuilder)));
        AddQueryBuilderScheme(new QueryBuilderScheme("MySql.EntityFrameworkCore", typeof(SqlBulkQueryBuilder)));
        AddQueryBuilderScheme(new QueryBuilderScheme("Microsoft.EntityFrameworkCore.SqlServer", typeof(SqlBulkQueryBuilder)));
        AddQueryBuilderScheme(new QueryBuilderScheme("Pomelo.EntityFrameworkCore.MySql", typeof(SqlBulkQueryBuilder)));

        _serviceProvider = serviceProvider;
    }

    public void AddQueryBuilderScheme(QueryBuilderScheme scheme)
    {
        if (_schemes.ContainsKey(scheme.Name))
        {
            throw new InvalidOperationException($"QueryBuilderScheme already exists: {scheme.Name}.");
        }

        lock (_lock)
        {
            _schemes.Add(scheme.Name, scheme);
        }
    }

    public Task<QueryBuilderScheme?> GetSchemeAsync(string name)
    {
        var contains = _schemes.TryGetValue(name, out var scheme);
        return Task.FromResult(contains ? scheme : null);
    }

    public async Task<ISqlQueryBuilder> GetQueryBuilderAsync(string name)
    {
        var scheme = await GetSchemeAsync(name) ?? throw new ArgumentException($"Cannot find query builder with name: {name}");

        var service = _serviceProvider.GetRequiredService(scheme.QueryBuilderType) as ISqlQueryBuilder;

        return service!;
    }
}

public class QueryBuilderScheme
{
    public string Name { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type QueryBuilderType { get; }

    public QueryBuilderScheme(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type queryBuilderType)
    {
        Name = name;
        QueryBuilderType = queryBuilderType;
    }
}
