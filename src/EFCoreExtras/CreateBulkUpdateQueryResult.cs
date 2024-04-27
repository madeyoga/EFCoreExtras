namespace EFCoreExtras;

public class CreateBulkUpdateQueryResult(string query, List<object> parameters, IEnumerable<string> ids)
{
    public string Query { get; } = query;
    public IEnumerable<object> Parameters { get; } = parameters;
    public IEnumerable<string> Ids { get; } = ids;
}
