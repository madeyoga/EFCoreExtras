namespace EFCoreExtras.Benchmarks;


public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Salary { get; set; } = 1000;

    public Employee()
    {

    }

    public Employee(int id, string name, decimal salary)
    {
        Id = id;
        Name = name;
        Salary = salary;
    }
}
