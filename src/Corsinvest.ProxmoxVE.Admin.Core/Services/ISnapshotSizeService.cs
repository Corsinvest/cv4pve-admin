/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface ISnapshotSizeService
{
    Task<IEnumerable<VmDiskInfo>> GetAsync(PveClient client);
}
