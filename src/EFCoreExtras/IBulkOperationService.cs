using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras;

public interface IBulkOperationService
{
    BulkInsertQueryResult CreateBulkInsertQuery<T>(DbContext context, List<T> objects) where T : class;
    BulkUpdateQueryResult CreateBulkUpdateQuery<T>(DbContext context, List<T> objects, string[] properties) where T : class;

    int ExecuteBulkInsert<T>(DbContext context, List<T> objects) where T : class;
    Task<int> ExecuteBulkInsertAsync<T>(DbContext context, List<T> objects) where T : class;

    //int ExecuteBulkUpdate<T>(DbContext context, BulkUpdateQueryResult result) where T : class;
    //Task<int> ExecuteBulkUpdateAsync<T>(DbContext context, BulkUpdateQueryResult result) where T : class;
}
