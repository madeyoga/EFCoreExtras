using Microsoft.Extensions.DependencyInjection;

namespace EFCoreExtras;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfCoreExtras(this IServiceCollection services)
    {
        // Bulk operation services
        services.AddSingleton<RelationalBulkOperationService>();
        services.AddSingleton<SqliteBulkOperationService>();

        services.AddSingleton<BulkOperationProvider>();

        return services;
    }
}
