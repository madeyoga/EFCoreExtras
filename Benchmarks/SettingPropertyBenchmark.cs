using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCoreExtras.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn(NumeralSystem.Arabic)]
public class SettingPropertyBenchmark
{
    [Params(10000)]
    public int NumberOfItems { get; set; }
    public List<Employee10> Employees { get; set; } = [];
    public PropertyInfo NameProperty { get; set; } = null!;
    public Action<Employee10, string> Setter { get; set; } = null!;

    private void PopulateEmployees()
    {
        for (int i = 0; i < NumberOfItems; i++)
        {
            var employee = new Employee10()
            {
                Id = i + 1,
                Name = i.ToString(),
            };

            Employees.Add(employee);
        }
    }

    [GlobalSetup(Target = nameof(DirectAssignment))]
    public void GlobalSetup_DirectAssignment()
    {
        PopulateEmployees();
    }

    [Benchmark(Baseline = true)]
    public void DirectAssignment()
    {
        foreach (var employee in Employees)
        {
            employee.Name = employee.Id.ToString();
        }
    }



    [GlobalSetup(Target = nameof(PropertyInfo_SetValue))]
    public void GlobalSetup_PropertyInfo_SetValue()
    {
        PopulateEmployees();
        NameProperty = typeof(Employee10).GetProperty("Name")!;
    }

    [Benchmark]
    public void PropertyInfo_SetValue()
    {
        foreach (var employee in Employees)
        {
            NameProperty.SetValue(employee, employee.Id.ToString());
        }
    }



    public static Action<TEntity, TKey> CreateSetter<TEntity, TKey>(string propertyName)
    {
        // delegate(instance, propertyValue)
        ParameterExpression instance = Expression.Parameter(typeof(TEntity), "instance");
        ParameterExpression propertyValue = Expression.Parameter(typeof(TKey), "propertyValue");

        // instance.PropertyName = propertyValue
        var body = Expression.Assign(Expression.Property(instance, propertyName), propertyValue);

        // Action (instance, propertValue) => instance.PropertyName = propertyValue
        return Expression.Lambda<Action<TEntity, TKey>>(body, instance, propertyValue).Compile();
    }

    [GlobalSetup(Targets = [nameof(CompiledLambdaExpression), nameof(CompiledLambdaExpression_WithCasting)])]
    public void GlobalSetup_CompiledLambdaExpression()
    {
        PopulateEmployees();
        NameProperty = typeof(Employee10).GetProperty("Name")!;
    }

    [Benchmark]
    public void CompiledLambdaExpression()
    {
        var setter = CreateSetter<Employee10, string>("Name");
        foreach (var employee in Employees)
        {
            setter(employee, employee.Id.ToString());
        }
    }

    [Benchmark]
    public void CompiledLambdaExpression_WithCasting()
    {
        object setter = CreateSetter<Employee10, string>("Name");
        foreach (var employee in Employees)
        {
            ((Action<Employee10, string>)setter)(employee, employee.Id.ToString());
        }
    }
}
