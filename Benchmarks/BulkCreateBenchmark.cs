using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreExtras.Benchmarks;

[MemoryDiagnoser]
//[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn(NumeralSystem.Arabic)]
public class BulkCreateBenchmark
{
    private readonly List<Employee> _data = [];
    private IServiceProvider _services = null!;

    // private IServiceScope _scope = null!;
    // private TestDbContext _context = null!;

    [Params(2000)]
    public int NumberOfItems {get;set;}

    [GlobalSetup]
    public void Setup()
    {
        for (int i = 0; i < NumberOfItems; i++)
        {
            var employee = new Employee
            {
                Name = i.ToString(),
                Salary = 1000 + i,
            };
            _data.Add(employee);
        }

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddEfCoreExtras();
        serviceCollection.AddDbContext<TestDbContext>(o =>
        {
            o.UseSqlite("DataSource=:memory:");
        });

        _services = serviceCollection.BuildServiceProvider();
    }

    // [IterationSetup]
    // public void IterationSetup()
    // {
    //     _scope = _services.CreateScope();
    //     _context = _scope.ServiceProvider.GetRequiredService<TestDbContext>();

    //     _context.Database.OpenConnection();
    //     _context.Database.EnsureCreated();
    // }

    // [IterationCleanup]
    // public void IterationCleanup()
    // {
    //     _context.Database.EnsureDeleted();
    //     _context.Database.CloseConnection();

    //     _context.Dispose();
    //     _scope.Dispose();
    // }

    // [Benchmark]
    // public int EFCore_SaveChanges()
    // {
    //    using var scope = _services.CreateScope();

    //    using var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

    //    context.Database.OpenConnection();
    //    context.Database.EnsureCreated();

    //    context.AddRange(_data);
    //    int affectedRows = context.SaveChanges();

    //    context.Database.EnsureDeleted();
    //    context.Database.CloseConnection();

    //    return affectedRows;
    // }

    [Benchmark(Baseline = true)]
    public int EFCoreExtras_BulkCreate_100BatchSize()
    {
        using var scope = _services.CreateScope();

        using var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        int affectedRows = context.BulkCreate(_data, batchSize: 100);

        context.Database.EnsureDeleted();
        context.Database.CloseConnection();

        return affectedRows;
    }
}
