using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreExtras.Benchmarks;

[MemoryDiagnoser]
//[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn(NumeralSystem.Arabic)]
[SimpleJob(RunStrategy.ColdStart, iterationCount: 1)]
public class BulkCreateBenchmark
{
    private readonly List<Employee> _data = [];
    private IServiceProvider _services = null!;

    //private IServiceScope _scope = null!;
    //private TestDbContext _context = null!;

    [Params(1000)]
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

    //[IterationSetup]
    //public void IterationSetup()
    //{
    //    _scope = _services.CreateScope();
    //    _context = _scope.ServiceProvider.GetRequiredService<TestDbContext>();

    //    _context.Database.OpenConnection();
    //    _context.Database.EnsureCreated();
    //}

    //[IterationCleanup]
    //public void IterationCleanup()
    //{
    //    _context.Database.EnsureDeleted();
    //    _context.Database.CloseConnection();

    //    _context.Dispose();
    //    _scope.Dispose();
    //}

    [Benchmark(Baseline = true)]
    public int EFCore_SaveChanges()
    {
        //_context.AddRange(_data);
        //return _context.SaveChanges();
        using var scope = _services.CreateScope();

        using var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        context.AddRange(_data);
        int affectedRows = context.SaveChanges();

        context.Database.EnsureDeleted();
        context.Database.CloseConnection();

        return affectedRows;
    }

    [Benchmark]
    public int EFCoreExtras_BulkCreate_100BatchSize()
    {
        //return _context.BulkCreate(_data);
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
