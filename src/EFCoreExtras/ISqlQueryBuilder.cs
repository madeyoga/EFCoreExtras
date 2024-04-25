namespace EFCoreExtras;

public interface ISqlQueryBuilder
{
    string BulkCreateQuery();
    string BulkUpdateQuery();
}
