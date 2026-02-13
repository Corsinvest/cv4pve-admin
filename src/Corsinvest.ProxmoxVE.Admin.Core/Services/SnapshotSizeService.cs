/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

internal class SnapshotSizeService : ISnapshotSizeService
{
    public Task<IEnumerable<VmDiskInfo>> GetAsync(PveClient client) => Task.FromResult(Enumerable.Empty<VmDiskInfo>());
}
