/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public partial class ResourceUsageGaugeStacked
{
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public double StartAngle { get; set; } = 0;
    [Parameter] public double EndAngle { get; set; } = 360;
    [Parameter] public IEnumerable<ResourceUsage> Data { get; set; } = [];

    private static readonly string[] Colors =
    [
        "var(--rz-series-1)",
        "var(--rz-series-2)",
        "var(--rz-series-3)",
        "var(--rz-series-4)",
        "var(--rz-series-5)",
        "var(--rz-series-6)",
        "var(--rz-series-7)",
        "var(--rz-series-8)",
    ];

    private static readonly string[] ColorsLight =
    [
        "color-mix(in srgb, var(--rz-series-1) 20%, transparent)",
        "color-mix(in srgb, var(--rz-series-2) 20%, transparent)",
        "color-mix(in srgb, var(--rz-series-3) 20%, transparent)",
        "color-mix(in srgb, var(--rz-series-4) 20%, transparent)",
        "color-mix(in srgb, var(--rz-series-5) 20%, transparent)",
        "color-mix(in srgb, var(--rz-series-6) 20%, transparent)",
        "color-mix(in srgb, var(--rz-series-7) 20%, transparent)",
        "color-mix(in srgb, var(--rz-series-8) 20%, transparent)",
    ];

    private List<ResourceUsage> Items { get; set; } = [];
    private double RadiusStep { get; set; }

    protected override void OnParametersSet()
    {
        Items = Data.ToList();
        RadiusStep = Items.Count > 1 ? 0.22 : 0.0;
    }
}
