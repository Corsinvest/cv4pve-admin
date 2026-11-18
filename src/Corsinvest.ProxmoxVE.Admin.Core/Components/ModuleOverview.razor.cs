/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

/// <summary>
/// Reusable module landing page: a mini-dashboard of the module widgets rendered through
/// <c>RadzenTileLayout</c>, then the list of features (<see cref="OverviewFeature"/>).
/// Widgets opt out by setting <see cref="ModuleWidget.ShowInOverview"/> to <c>false</c>.
/// </summary>
public partial class ModuleOverview<TModule> where TModule : ModuleBase
{
    [CascadingParameter(Name = nameof(ClusterName))] public string? ClusterName { get; set; }

    [Parameter] public RenderFragment? Features { get; set; }

    /// <summary>Grid column count (same as the Dashboard module).</summary>
    protected const int GridCols = 24;

    /// <summary>Pixel height per grid row.</summary>
    protected const int RowHeightPx = 40;

    protected ModuleBase? Module { get; private set; }

    /// <summary>Visible widgets packed greedily into 1-based (col, row) positions.</summary>
    protected List<Tile> Tiles { get; private set; } = [];

    protected sealed record Tile(int Col, int Row, int ColSpan, int RowSpan, string Title, string? Description, Type Type, Dictionary<string, object> Parameters);

    private bool _clusterRead = true;

    protected override async Task OnParametersSetAsync()
    {
        Module = IModuleService.GetCached<TModule>();
        _clusterRead = string.IsNullOrEmpty(ClusterName) || await PermissionService.HasClusterReadAsync(ClusterName);
        Tiles = BuildTiles();
    }

    /// <summary>
    /// Greedy bin-packing in declared order in a <see cref="GridCols"/>-wide grid.
    /// Returns 1-based (Col, Row) positions ready for <c>RadzenTileLayoutItem</c>.
    /// </summary>
    private List<Tile> BuildTiles()
    {
        var tiles = new List<Tile>();
        if (Module is null) { return tiles; }

        var col = 1;
        var row = 1;
        var rowHeight = 0;

        foreach (var widget in Module.Widgets.Where(w => w.ShowInOverview && (_clusterRead || !w.ClusterWide)))
        {
            var colSpan = Math.Clamp(widget.Width, 1, GridCols);
            var rowSpan = Math.Max(1, widget.Height);

            if (col + colSpan - 1 > GridCols)
            {
                row += rowHeight;
                col = 1;
                rowHeight = 0;
            }

            tiles.Add(new Tile(col, row, colSpan, rowSpan, widget.Name, widget.Description, widget.RenderInfo.Type, BuildParameters(widget)));

            col += colSpan;
            rowHeight = Math.Max(rowHeight, rowSpan);
        }

        return tiles;
    }

    private Dictionary<string, object> BuildParameters(ModuleWidget widget)
    {
        var parameters = new Dictionary<string, object>();
        var renderType = widget.RenderInfo.Type;

        if (renderType.GetProperty("ClusterNames") != null)
        {
            parameters["ClusterNames"] = string.IsNullOrEmpty(ClusterName)
                                            ? Array.Empty<string>()
                                            : [ClusterName];
        }

        if (widget.RenderSettingsInfo != null && renderType.GetProperty("Settings") != null)
        {
            var settings = Activator.CreateInstance(widget.RenderSettingsInfo.Type);
            if (settings != null) { parameters["Settings"] = settings; }
        }

        if (widget.RenderInfo.Parameters != null)
        {
            foreach (var (key, value) in widget.RenderInfo.Parameters)
            {
                parameters[key] = value;
            }
        }

        return parameters;
    }
}
