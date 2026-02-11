using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDbContextFactoryPostgreSql<TContext>(this IServiceCollection services,
                                                                             string schemName) where TContext : DbContext
    {
        services.AddDbContextFactory<TContext>((sp, builder) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")!;

#if DEBUG
            builder.EnableDetailedErrors();
            builder.EnableSensitiveDataLogging();

            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(ServiceExtensions));

            logger.LogDebug("ConnectionString: {connectionString}", connectionString);
#endif
            builder.UseNpgsql(connectionString, options => options.MigrationsHistoryTable("__EFMigrationsHistory", schemName));
        });

        return services;
    }
}
