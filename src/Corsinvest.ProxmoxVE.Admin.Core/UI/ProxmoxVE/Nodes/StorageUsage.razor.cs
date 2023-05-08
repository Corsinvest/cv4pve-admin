/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Nodes;

public partial class StorageUsage
{
    [EditorRequired][Parameter] public Func<Task<IEnumerable<NodeStorageContent>>> GetContents { get; set; } = default!;
    [EditorRequired][Parameter] public Func<Task<IEnumerable<ClusterResource>>> GetStorages { get; set; } = default!;
    [Parameter] public string GroupBy { get; set; } = nameof(NodeStorageContent.Storage);

    [Parameter]
    public IEnumerable<string> PropertiesName { get; set; } = new[]
    {
        nameof(NodeStorageContent.Storage),
        nameof(NodeStorageContent.FileName),
        nameof(NodeStorageContent.Size),
        nameof(NodeStorageContent.Creation),
        nameof(NodeStorageContent.Format),
        nameof(NodeStorageContent.Content),
        nameof(NodeStorageContent.Verified),
        nameof(NodeStorageContent.Encrypted),
    };

    private IEnumerable<NodeStorageContent> Contents { get; set; } = default!;
    private IEnumerable<ClusterResource> Infos { get; set; } = default!;
    private bool IsGroupByStorage => GroupBy == nameof(NodeStorageContent.Storage);

    protected override async Task OnInitializedAsync()
    {
        Contents = (await GetContents())
                    .DistinctBy(a => a.Volume)
                    .OrderBy(a => a.Content)
                    .ThenBy(a => a.FileName);

        Infos = await GetStorages();
    }
}