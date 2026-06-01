/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.Core.Persistence;

/// <summary>
/// PostgreSQL-specific implementation of <see cref="IModuleMaintenance"/>.
/// Operates against a single schema (auto-detected from the DbContext's default schema)
/// — never the whole database — so each module's maintenance is independent.
/// </summary>
public class PostgreSqlModuleMaintenance(DbContext context) : IModuleMaintenance
{
    private readonly string _schema = context.Model.GetDefaultSchema()
                  ?? throw new InvalidOperationException(
                      $"DbContext '{context.GetType().Name}' has no default schema set. Add modelBuilder.HasDefaultSchema(\"...\") in OnModelCreating.");

    /// <inheritdoc />
    public async Task ExecuteDatabaseMaintenanceAsync(DatabaseMaintenanceOperation operation, CancellationToken cancellationToken = default)
    {
        // PostgreSQL does not allow parametrising identifiers; the schema and table names are
        // already passed through PostgreSqlHelper.QuoteIdentifier, so EF1002 is a false positive.
        var schema = PostgreSqlHelper.QuoteIdentifier(_schema);

        switch (operation)
        {
            case DatabaseMaintenanceOperation.Reindex:
#pragma warning disable EF1002
                await context.Database.ExecuteSqlRawAsync($"REINDEX SCHEMA {schema}", cancellationToken);
#pragma warning restore EF1002
                return;

            case DatabaseMaintenanceOperation.Optimize:
            case DatabaseMaintenanceOperation.Compact:
                var verb = operation == DatabaseMaintenanceOperation.Compact ? "VACUUM (FULL, ANALYZE)" : "VACUUM (ANALYZE)";
                foreach (var table in await GetSchemaTableNamesAsync(cancellationToken))
                {
#pragma warning disable EF1002
                    await context.Database.ExecuteSqlRawAsync($"{verb} {schema}.{PostgreSqlHelper.QuoteIdentifier(table)}", cancellationToken);
#pragma warning restore EF1002
                }
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown maintenance operation");
        }
    }

    /// <inheritdoc />
    public async Task<long> GetDatabaseSize(CancellationToken cancellationToken = default)
    {
        var conn = context.Database.GetDbConnection();
        if (conn.State != global::System.Data.ConnectionState.Open) { await conn.OpenAsync(cancellationToken); }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COALESCE(SUM(pg_total_relation_size(format('%I.%I', schemaname, tablename))), 0)::bigint
            FROM pg_tables WHERE schemaname = @schema
            """;
        var p = cmd.CreateParameter();
        p.ParameterName = "@schema";
        p.Value = _schema;
        cmd.Parameters.Add(p);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is long l ? l : 0L;
    }

    private async Task<IReadOnlyList<string>> GetSchemaTableNamesAsync(CancellationToken cancellationToken)
    {
        var conn = context.Database.GetDbConnection();
        if (conn.State != global::System.Data.ConnectionState.Open) { await conn.OpenAsync(cancellationToken); }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT tablename FROM pg_tables WHERE schemaname = @schema ORDER BY tablename";
        var p = cmd.CreateParameter();
        p.ParameterName = "@schema";
        p.Value = _schema;
        cmd.Parameters.Add(p);

        var tables = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }
}
