/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;

namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public interface IModuleMaintenance
{
    Task ExecuteDatabaseMaintenanceAsync(DatabaseMaintenanceOperation operation, CancellationToken cancellationToken = default);
    Task<long> GetDatabaseSize(CancellationToken cancellationToken = default);
}
