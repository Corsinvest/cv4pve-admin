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
    /// Optimize database - reclaims storage and improves performance
    /// </summary>
    Optimize,

    /// <summary>
    /// Reindex database - rebuilds all indexes
    /// </summary>
    Reindex
}
