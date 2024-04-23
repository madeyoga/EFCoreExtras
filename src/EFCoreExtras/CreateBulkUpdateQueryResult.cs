namespace EFCoreExtras;

public class CreateBulkUpdateQueryResult(string query, List<object> parameters, ISet<string> ids)
{
    public string Query { get; } = query;
    public IEnumerable<object> Parameters { get; } = parameters;
    public ISet<string> Ids { get; } = ids;
}
