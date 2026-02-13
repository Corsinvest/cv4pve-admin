/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;

public interface IPveClientFactory
{
    Task<PveClient> CreateClientAsync(ClusterSettings settings, CancellationToken cancellationToken);
}
