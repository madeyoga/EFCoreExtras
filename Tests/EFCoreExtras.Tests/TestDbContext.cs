using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Tests;


public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public virtual DbSet<Item> Items { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Invoice> Invoices { get; set; }
}
