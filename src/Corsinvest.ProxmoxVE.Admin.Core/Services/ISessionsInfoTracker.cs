/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface ISessionsInfoTracker
{
    IEnumerable<SessionInfo> Sessions { get; }
    event EventHandler OnChanged;
    void UpdateCurrentPage(string circuitId, string currentPage);
    void SetHubConnectionId(string circuitId, string hubConnectionId);
}
