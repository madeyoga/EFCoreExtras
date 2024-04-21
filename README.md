# EFCoreExtras

Bulk extensions for Entity Framework Core.

## Features

### Bulk extensions

Bulk operations typically reduce the number of database round-trips required. This make bulk operations can often be faster than calling `DbContext.SaveChangesAsync()`.

- `BulkCreateAsync`
- `BulkUpdateAsync`

### Others

Easily paginate `IQueryable` with `Pagination` class.

- `Pagination`


## Usage

### BulkCreateAsync

```cs
List<Employee> employees = [];

foreach (int i = 0; i < 1000; i++)
{
    var employee = new Employee
    {
        Name = "Name",
        Salary = 1000,
    };

    employees.Add(employee);
}

await context.BulkCreateAsync(employees, batchSize: 500);
```

```sql
INSERT INTO Employees (Id, Name, Salary)
VALUES (..., 'Name', 1000),
       (..., 'Name', 1000),
       (..., 'Name', 1000)
```

### BulkUpdateAsync

```cs
var employees = context.Employees.ToList();

foreach (var employee in context.Employees)
{
    employee.Name = $"{employee.Name} {employee.Id}"
    employee.Salary += 1000;
}

await context.BulkUpdateAsync(employees);
```

```sql
UPDATE Employees 
SET Name = CASE 
    WHEN Id = 1 THEN '...'
    WHEN Id = 2 THEN '...'
    ...
    ELSE Name,

    Salary = CASE
    WHEN Id = 1 THEN ...
    WHEN Id = 2 THEN ...
    ...
    ELSE Salary
END
WHERE id IN (1, 2, ...)
```
