using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreExtras.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn(NumeralSystem.Arabic)]
public class BulkUpdateBenchmark
{
    private readonly List<Employee10> _data = [];
    private IServiceProvider _services = null!;

    private IServiceScope _scope = null!;
    private TestDbContext _context = null!;

    [Params(2000)]
    public int NumberOfItems { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        for (int i = 0; i < NumberOfItems; i++)
        {
            var employee = new Employee10
            {
                Name = i.ToString(),
                Age = 20,
                Email = "test@email.com",
                Address = "None",
                DateOfBirth = DateTime.UtcNow,
                IsActive = true,
                Salary = 1000 + i,
                Department = "Software",
                Position = "Junior",
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

    [IterationSetup]
    public void IterationSetup()
    {
        _scope = _services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<TestDbContext>();

        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _context.AddRange(_data);
        _context.SaveChanges();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Database.CloseConnection();

        _context.Dispose();
        _scope.Dispose();
    }

    [Benchmark(Baseline = true)]
    public int EFCoreExtras_BulkUpdate()
    {
        foreach (var item in _data)
        {
            item.Name = $"{item.Name}__{item.Id}";
            item.Salary += 1000;
        }

        return _context.BulkUpdate(_data, [
            "Name", 
            "Age", 
            "Email",
            "Address",
            "DateOfBirth",
            "IsActive",
            "Salary",
            "Department",
            "Position",
        ]);
    }

    [Benchmark]
    public int EFCoreExtras_BulkUpdate_Typed()
    {
        foreach (var item in _data)
        {
            item.Name = $"{item.Name}__{item.Id}";
            item.Salary += 1000;
        }

        return _context.BulkUpdate(_data, [
            e => e.Name, 
            e => e.Age,
            e => e.Email!,
            e => e.Address!,
            e => e.DateOfBirth,
            e => e.IsActive,
            e => e.Salary,
            e => e.Department!,
            e => e.Position!,
        ]);
    }

    [Benchmark]
    public int EFCore_SaveChanges()
    {
        foreach (var item in _data)
        {
            item.Name = $"{item.Name}__{item.Id}";
            item.Salary += 1000;
        }

        return _context.SaveChanges();
    }
}
