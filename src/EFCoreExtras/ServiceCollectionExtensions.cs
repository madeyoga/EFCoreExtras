using Microsoft.Extensions.DependencyInjection;

namespace EFCoreExtras;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfCoreExtras(this IServiceCollection services)
    {
        services.AddSingleton<QueryBuilderProvider>();
        services.AddSingleton<SqlBulkQueryBuilder>();

        return services;
    }
}
