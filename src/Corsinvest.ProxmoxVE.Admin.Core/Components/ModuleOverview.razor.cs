/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.WidgetGrid;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

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

    protected List<WidgetGridItem> GridItems { get; private set; } = [];

    protected int TotalRows { get; private set; }

    protected override void OnInitialized()
    {
        Module = IModuleService.GetCached<TModule>();
        BuildGrid();
    }

    /// <summary>
    /// Auto-positions visible widgets in a greedy 12-col grid in declared order:
    /// fills the current row left-to-right, opens a new row when the next widget doesn't fit.
    /// </summary>
    private void BuildGrid()
    {
        GridItems = [];
        var col = 0;
        var row = 0;
        var rowHeight = 0;
        var id = 0;

        foreach (var widget in Module?.Widgets.Where(w => w.ShowInOverview) ?? [])
        {
            var colSpan = Math.Clamp(widget.Width, 1, GridCols);
            var rowSpan = Math.Max(1, widget.Height);

            if (col + colSpan > GridCols)
            {
                row += rowHeight;
                col = 0;
                rowHeight = 0;
            }

            GridItems.Add(new WidgetGridItem
            {
                Id = id++,
                Col = col,
                Row = row,
                ColSpan = colSpan,
                RowSpan = rowSpan,
                Template = BuildTemplate(widget),
            });

            col += colSpan;
            rowHeight = Math.Max(rowHeight, rowSpan);
        }

        TotalRows = row + rowHeight;
    }

    private RenderFragment BuildTemplate(ModuleWidget widget) => builder =>
    {
        builder.OpenComponent(0, typeof(DynamicComponent));
        builder.AddAttribute(1, "Type", widget.RenderInfo.Type);
        builder.AddAttribute(2, "Parameters", BuildParameters(widget));
        builder.CloseComponent();
    };

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
