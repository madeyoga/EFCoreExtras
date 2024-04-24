using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Mathematics;
using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn(NumeralSystem.Arabic)]
public class BulkCreateBenchmark
{
    private List<Employee> _data = [];

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
    }

    [Benchmark(Baseline = true)]
    public int EFCoreExtras_BulkCreate_100BatchSize()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite($"DataSource=:memory:")
            .Options;

        using var _dbContext = new TestDbContext(options);

        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        int writtenRows = _dbContext.BulkCreate(_data, batchSize: 100);
        
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.CloseConnection();

        return writtenRows;
    }
    
    [Benchmark]
    public int EFCoreExtras_BulkCreate()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite($"DataSource=:memory:")
            .Options;

        using var _dbContext = new TestDbContext(options);

        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        int writtenRows = _dbContext.BulkCreate(_data);
        
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.CloseConnection();

        return writtenRows;
    }

    [Benchmark]
    public int EFCore_SaveChanges()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite($"DataSource=:memory:")
            .Options;

        using var _dbContext = new TestDbContext(options);

        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _dbContext.AddRange(_data);
        int writtenRows = _dbContext.SaveChanges();
        
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.CloseConnection();

        return writtenRows;
    }
}
