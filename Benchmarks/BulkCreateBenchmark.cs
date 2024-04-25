using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Mathematics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static System.Formats.Asn1.AsnWriter;

namespace EFCoreExtras.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn(NumeralSystem.Arabic)]
public class BulkCreateBenchmark
{
    private readonly List<Employee> _data = [];
    private IServiceProvider _services = null!;
    private IServiceScope _scope = null!;
    private TestDbContext _context = null!;

    [GlobalSetup]
    public void Setup()
    {
        for (int i = 0; i < 2000; i++)
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
        _scope = _services.CreateScope();

        _context = _services.GetRequiredService<TestDbContext>();

        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        Console.WriteLine("GLOBAL SETUP!");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Database.CloseConnection();
        _context.Dispose();
        _scope.Dispose();

        Console.WriteLine("GLOBAL CLEANUP! HALLO!");
    }

    [Benchmark(Baseline = true)]
    public int EFCoreExtras_BulkCreate_100BatchSize()
    {
        return _context.BulkCreate(_data, batchSize: 100);
    }

    [Benchmark]
    public int EFCore_SaveChanges()
    {
        _context.AddRange(_data);
        return _context.SaveChanges();
    }
}
