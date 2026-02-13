/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.Core.Persistence;

/// <summary>
/// Base class for module DbContext to eliminate code duplication
/// </summary>
/// <typeparam name="T">The concrete DbContext type</typeparam>
public abstract class ModuleDbContextBase<T>(DbContextOptions<T> options) : DbContext(options) where T : ModuleDbContextBase<T>
{
    /// <summary>
    /// The schema name for this module (e.g., "autosnap", "dashboard")
    /// </summary>
    protected abstract string SchemaName { get; }

    /// <summary>
    /// Whether to use PostgreSQL case insensitive collation (default: true)
    /// </summary>
    protected virtual bool UsePostgreSqlCollation => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (!string.IsNullOrEmpty(SchemaName)) { modelBuilder.HasDefaultSchema(SchemaName); }
        if (UsePostgreSqlCollation) { modelBuilder.ApplyPostgreSqlCaseInsensitiveCollation(); }

        ConfigureEntities(modelBuilder);
    }

    /// <summary>
    /// Configure module-specific entities, indexes, and relationships
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected abstract void ConfigureEntities(ModelBuilder modelBuilder);

    public async Task ExecuteMaintenanceAsync(DatabaseMaintenanceOperation operation, CancellationToken cancellationToken = default)
        => await Database.ExecuteSqlRawAsync(operation switch
        {
            // PostgreSQL specific commands (can be extended for other databases)
            DatabaseMaintenanceOperation.Optimize => "VACUUM ANALYZE",
            DatabaseMaintenanceOperation.Reindex => $"REINDEX DATABASE \"{Database.GetDbConnection().Database}\"",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown maintenance operation")
        }, cancellationToken);
}
