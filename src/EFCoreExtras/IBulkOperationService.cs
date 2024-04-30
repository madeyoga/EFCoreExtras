using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras;

public interface IBulkOperationService
{
    int ExecuteBulkInsert<T>(DbContext context, IEnumerable<T> objects) where T : class;
    Task<int> ExecuteBulkInsertAsync<T>(DbContext context, IEnumerable<T> objects) where T : class;

    int ExecuteBulkInsertRetrieveKeys<T>(DbContext context, IEnumerable<T> objects, int batchSize) where T : class;
    Task<int> ExecuteBulkInsertRetrieveKeysAsync<T>(DbContext context, IEnumerable<T> objects, int batchSize) where T : class;

    int ExecuteBulkUpdate<T>(DbContext context, IEnumerable<T> objects, string[] properties) where T : class;
    Task<int> ExecuteBulkUpdateAsync<T>(DbContext context, IEnumerable<T> objects, string[] properties) where T : class;
}
