namespace EFCoreExtras;

public class BulkInsertQueryResult(string query, object[] parameters, string pkName)
{
    public string Query { get; } = query;
    public object[] Parameters { get; } = parameters;
    public string PrimaryKeyPropertyName { get; } = pkName;
}