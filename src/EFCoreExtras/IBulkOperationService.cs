using Microsoft.EntityFrameworkCore;

namespace EFCoreExtras;

public interface IBulkOperationService
{
    int ExecuteBulkInsert<T>(DbContext context, IEnumerable<T> objects) where T : class;
    Task<int> ExecuteBulkInsertAsync<T>(DbContext context, IEnumerable<T> objects) where T : class;

    T[] ExecuteBulkInsertRetrieve<T>(DbContext context, IEnumerable<T> objects, int batchSize = 100) where T : class;
    Task<T[]> ExecuteBulkInsertRetrieveAsync<T>(DbContext context, IEnumerable<T> objects, int batchSize = 100) where T : class;

    int ExecuteBulkUpdate<T>(DbContext context, IEnumerable<T> objects, string[] properties) where T : class;
    Task<int> ExecuteBulkUpdateAsync<T>(DbContext context, IEnumerable<T> objects, string[] properties) where T : class;
}
