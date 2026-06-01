/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Persistence;

/// <summary>
/// Database maintenance operations
/// </summary>
public enum DatabaseMaintenanceOperation
{
    /// <summary>
    /// Optimize (VACUUM ANALYZE) — refresh statistics and mark dead rows reusable.
    /// Concurrent read/write friendly; does not lock the table and does not shrink the file on disk.
    /// </summary>
    Optimize,

    /// <summary>
    /// Reindex (REINDEX SCHEMA) — rebuild every index in the module's schema.
    /// Holds a short lock per index; safe for routine maintenance.
    /// </summary>
    Reindex,

    /// <summary>
    /// Compact (VACUUM FULL) — rewrite each table from scratch and return reclaimed space to the
    /// filesystem. Holds an ACCESS EXCLUSIVE lock for the whole operation: tables cannot be read
    /// or written while it runs. Requires temporary disk space roughly equal to the biggest table.
    /// Use after massive cleanups (audit logs, tasks, old scans) — not as routine maintenance.
    /// </summary>
    Compact
}
