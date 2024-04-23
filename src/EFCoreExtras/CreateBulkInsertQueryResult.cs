namespace EFCoreExtras;

public class CreateBulkInsertQueryResult(string query, IEnumerable<object> parameters)
{
    public string Query { get; } = query;
    public IEnumerable<object> Parameters { get; } = parameters;
}