using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Tests;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public virtual DbSet<Item> Items { get; set; }
    public virtual DbSet<ItemString> ItemStrings { get; set; }
    public virtual DbSet<Employee3> Employees { get; set; }
    public virtual DbSet<Employee10> Employees10 { get; set; }
    public virtual DbSet<Employee20> Employees20 { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }
}
