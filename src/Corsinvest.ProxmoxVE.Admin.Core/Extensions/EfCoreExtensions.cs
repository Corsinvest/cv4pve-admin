/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class EfCoreExtensions
{
    public static async Task<int> AddOrUpdateAsync<T>(this DbContext dbContext, T entity) where T : IId
    {
        if (entity.Id == 0)
        {
            await dbContext.AddAsync(entity);
        }
        else
        {
            dbContext.Update(entity);
        }
        return await dbContext.SaveChangesAsync();
    }

    public static async Task MigrateDbAsync<TContext>(this IServiceScope scope) where TContext : DbContext
    {
        if (!ApplicationHelper.IsRunningInEfTool)
        {
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection")!;

            try
            {
                await InitializeDbPgAsync(connectionString);
            }
            catch (PostgresException ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
                logger.LogWarning(ex, "Unable to initialize PostgreSQL extensions or collations. Some features may not be available.");
            }

            await using var dbContext = await scope.GetDbContextAsync<TContext>();
            await dbContext.Database.MigrateAsync();
        }
    }

    private static async Task InitializeDbPgAsync(string connectionString)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        var sql = """
            CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
            CREATE EXTENSION IF NOT EXISTS "citext";

            CREATE COLLATION IF NOT EXISTS case_insensitive (
                provider = icu,
                locale = 'en-u-ks-primary',
                deterministic = false
            );

            CREATE COLLATION IF NOT EXISTS case_accent_insensitive (
                provider = icu,
                locale = 'und-u-ks-level1',
                deterministic = false
            );
        """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}
