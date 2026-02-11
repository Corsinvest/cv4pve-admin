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
