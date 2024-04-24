using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Benchmarks;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public virtual DbSet<Employee> Employees { get; set; }
}