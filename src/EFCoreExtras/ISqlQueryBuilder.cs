using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras;

public interface ISqlQueryBuilder
{
    CreateBulkInsertQueryResult CreateBulkInsertQuery<T>(DbContext context, List<T> objects) where T : class;
    CreateBulkUpdateQueryResult CreateBulkUpdateQuery<T>(DbContext context, List<T> objects, string[] properties) where T : class;
}
