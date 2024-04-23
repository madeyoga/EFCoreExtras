using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Benchmarks;

public class BulkCreateBenchmark
{
    private TestDbContext _dbContext = null!;
    private DbContextOptions<TestDbContext> options = null!;

    [GlobalSetup]
    public void Setup()
    {
        options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:") // Using an in-memory database for testing
            .Options;

        _dbContext = new TestDbContext(options);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public virtual DbSet<Employee> Employees { get; set; }
}