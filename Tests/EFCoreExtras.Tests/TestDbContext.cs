using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras.Tests;


public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public virtual DbSet<Item> Items { get; set; }
}
