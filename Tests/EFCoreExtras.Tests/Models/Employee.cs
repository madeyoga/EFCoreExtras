namespace EFCoreExtras.Tests;

public class Employee
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public decimal Salary { get; set; } = 1000m;
}
