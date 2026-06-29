/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

/// <summary>
/// Reusable module landing page: the standard three-column intro on top
/// plus a fixed mini-dashboard rendered through <c>RadzenTileLayout</c>.
/// Each module gets its overview by adding a single tag and the three text fragments.
/// Widgets opt out by setting <see cref="ModuleWidget.ShowInOverview"/> to <c>false</c>.
/// </summary>
public partial class ModuleOverview<TModule> where TModule : ModuleBase
{
    [CascadingParameter(Name = nameof(ClusterName))] public string? ClusterName { get; set; }

    [Parameter] public RenderFragment? Content1 { get; set; }
    [Parameter] public RenderFragment? Content2 { get; set; }
    [Parameter] public RenderFragment? Content3 { get; set; }

    [Parameter] public RenderFragment? Block1 { get; set; }
    [Parameter] public RenderFragment? Block2 { get; set; }
    [Parameter] public RenderFragment? Block3 { get; set; }

    [Parameter] public string Icon1 { get; set; } = "overview";
    [Parameter] public string Icon2 { get; set; } = "thumb_up";
    [Parameter] public string Icon3 { get; set; } = "self_improvement";

    [Parameter] public string Title1 { get; set; } = "Overview";
    [Parameter] public string Title2 { get; set; } = "Benefit";
    [Parameter] public string Title3 { get; set; } = "Experience";

    /// <summary>Grid column count (same as the Dashboard module).</summary>
    protected const int GridCols = 12;

    /// <summary>Pixel height per grid row.</summary>
    protected const int RowHeightPx = 60;

    protected ModuleBase? Module { get; private set; }

    /// <summary>Visible widgets packed greedily into 1-based (col, row) positions.</summary>
    protected List<Tile> Tiles { get; private set; } = [];

    protected sealed record Tile(int Col, int Row, int ColSpan, int RowSpan, Type Type, Dictionary<string, object> Parameters);

    protected override void OnInitialized()
    {
        Module = IModuleService.GetCached<TModule>();
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

        foreach (var widget in Module.Widgets.Where(w => w.ShowInOverview))
        {
            var colSpan = Math.Clamp(widget.Width, 1, GridCols);
            var rowSpan = Math.Max(1, widget.Height);

            if (col + colSpan - 1 > GridCols)
            {
                row += rowHeight;
                col = 1;
                rowHeight = 0;
            }

            tiles.Add(new Tile(col, row, colSpan, rowSpan, widget.RenderInfo.Type, BuildParameters(widget)));

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
