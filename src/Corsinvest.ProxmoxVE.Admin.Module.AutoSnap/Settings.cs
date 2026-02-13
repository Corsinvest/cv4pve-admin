/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json.Serialization;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Notifier;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap;

public class Settings : IModuleSettings, INotifierConfigurationsSettings
{
    [Required] public string ClusterName { get; set; } = default!;
    public bool Enabled { get; set; }

    [Range(0, int.MaxValue)]
    public int KeepHistory { get; set; } = 10;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Notify Notify { get; set; } = Notify.None;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SearchMode SearchMode { get; set; } = SearchMode.Managed;

    public string TimestampFormat { get; set; } = ProxmoxVE.AutoSnap.Api.Application.DefaultTimestampFormat;

    public bool OnRemoveJobRemoveSnapshots { get; set; } = true;

    [Range(0, 100)]
    public int MaxPercentageStorage { get; set; } = 95;

    public IEnumerable<string> NotifierConfigurations { get; set; } = [];
}
