using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace EFCoreExtras;

public class BulkOperationProvider
{
    private readonly Dictionary<string, BulkOperationScheme> _schemes = new(StringComparer.Ordinal);
    private readonly object _lock = new();
    private readonly IServiceProvider serviceProvider;

    public BulkOperationProvider(IServiceProvider serviceProvider)
    {
        AddBulkOperationScheme(new BulkOperationScheme("Microsoft.EntityFrameworkCore.Sqlite", typeof(SqliteBulkOperationService)));
        AddBulkOperationScheme(new BulkOperationScheme("Npgsql.EntityFrameworkCore.PostgreSQL", typeof(RelationalBulkOperationService)));
        AddBulkOperationScheme(new BulkOperationScheme("MySql.EntityFrameworkCore", typeof(RelationalBulkOperationService)));
        AddBulkOperationScheme(new BulkOperationScheme("Microsoft.EntityFrameworkCore.SqlServer", typeof(RelationalBulkOperationService)));
        AddBulkOperationScheme(new BulkOperationScheme("Pomelo.EntityFrameworkCore.MySql", typeof(RelationalBulkOperationService)));

        this.serviceProvider = serviceProvider;
    }

    public void AddBulkOperationScheme(BulkOperationScheme scheme)
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

    public Task<BulkOperationScheme?> GetSchemeAsync(string name)
    {
        var contains = _schemes.TryGetValue(name, out var scheme);
        return Task.FromResult(contains ? scheme : null);
    }

    public BulkOperationScheme? GetScheme(string name)
    {
        return _schemes.GetValueOrDefault(name);
    }

    public async Task<IBulkOperationService?> GetBulkOperationServiceAsync(string name)
    {
        var scheme = await GetSchemeAsync(name) ?? throw new ArgumentException($"Cannot find query builder with name: {name}");

        var instance = (serviceProvider.GetService(scheme.BulkOperationServiceType) ?? 
            ActivatorUtilities.CreateInstance(serviceProvider, scheme.BulkOperationServiceType))
            as IBulkOperationService;

        return instance;
    }

    public IBulkOperationService? GetBulkOperationService(string name)
    {
        var scheme = GetScheme(name) ?? throw new ArgumentException($"Cannot find query builder with name: {name}");

        var instance = (serviceProvider.GetService(scheme.BulkOperationServiceType) ??
            ActivatorUtilities.CreateInstance(serviceProvider, scheme.BulkOperationServiceType))
            as IBulkOperationService;

        return instance;
    }

    public async Task<IBulkOperationService> CreateBulkOperationServiceAsync(string name)
    {
        var scheme = await GetSchemeAsync(name) ?? throw new ArgumentException($"Cannot find query builder with name: {name}");

        var instance = Activator.CreateInstance(scheme.BulkOperationServiceType) as IBulkOperationService;

        return instance!;
    }

    public IBulkOperationService CreateBulkOperationService(string name)
    {
        var scheme = GetScheme(name) ?? throw new ArgumentException($"Cannot find query builder with name: {name}");

        var instance = Activator.CreateInstance(scheme.BulkOperationServiceType) as IBulkOperationService;

        return instance!;
    }
}

public class BulkOperationScheme
{
    public string Name { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type BulkOperationServiceType { get; }

    public BulkOperationScheme(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type bulkOperationServiceType)
    {
        Name = name;
        BulkOperationServiceType = bulkOperationServiceType;
    }
}
