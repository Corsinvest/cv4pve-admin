/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public partial class ResourceCard
{
    private record HistoryPoint(int X, double Y);

    private readonly Queue<HistoryPoint> CpuHistory = new();
    private readonly Queue<HistoryPoint> RamHistory = new();
    private int _historyIndex;
    private const int MaxHistory = 20;

    private bool IsOnline => Resource.IsRunning || Resource.IsOnline;

    private BadgeStyle StatusBadgeStyle
        => IsOnline
            ? BadgeStyle.Success
            : BadgeStyle.Danger;

    protected override void OnParametersSet()
    {
        if (Resource.ResourceType == ClusterResourceType.Node || Resource.ResourceType == ClusterResourceType.Vm)
        {
            Enqueue(CpuHistory, Resource.CpuUsagePercentage * 100);
            Enqueue(RamHistory, Resource.MemoryUsagePercentage * 100);
            _historyIndex++;
        }
    }

    private void Enqueue(Queue<HistoryPoint> queue, double value)
    {
        if (queue.Count >= MaxHistory) { queue.Dequeue(); }
        queue.Enqueue(new HistoryPoint(_historyIndex, value));
    }
}
