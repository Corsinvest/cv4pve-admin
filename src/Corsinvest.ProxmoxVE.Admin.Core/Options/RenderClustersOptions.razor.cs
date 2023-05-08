/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Options;

public partial class RenderClustersOptions
{
    [EditorRequired][Parameter] public AdminOptions Options { get; set; } = default!;
    [Inject] private IDataGridManager<ClusterOptions> DataGridManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Clusters"];
        DataGridManager.DefaultSort = new() { [nameof(ClusterOptions.Name)] = false };
        DataGridManager.QueryAsync = async () => await Task.FromResult(Options.Clusters);

        DataGridManager.SaveAsync = async (item, isNew) =>
        {
            if (isNew) { Options.Clusters.Add(item); }
            return await Task.FromResult(true);
        };

        DataGridManager.DeleteAsync = async (items) =>
        {
            foreach (var item in items) { Options.Clusters.Remove(item); }
            return await Task.FromResult(true);
        };
    }
}