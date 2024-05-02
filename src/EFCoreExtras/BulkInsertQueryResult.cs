namespace EFCoreExtras;

public class BulkInsertQueryResult(string query, object[] parameters)
{
    public string Query { get; } = query;
    public object[] Parameters { get; } = parameters;
}