/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json.Serialization;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Notifier;
using Corsinvest.ProxmoxVE.AutoSnap.Api;

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

    [Required, CustomValidation(typeof(Settings), nameof(ValidateTimestampFormat))]
    public string TimestampFormat { get; set; } = AutoSnapEngine.DefaultTimestampFormat;

    // The engine refuses a format it cannot read back, on every snap, clean and status:
    // checked here, so it is refused when saved and not when the job runs.
    public static ValidationResult? ValidateTimestampFormat(string? value, ValidationContext context)
        => AutoSnapEngine.ValidateLabelAndTimestampFormat(string.Empty, value ?? string.Empty) is { } error
            ? new ValidationResult(error, [context.MemberName!])
            : ValidationResult.Success;

    public bool OnRemoveJobRemoveSnapshots { get; set; } = true;

    // 0 would skip every guest on a storage that is not empty.
    [Range(1, 100)]
    public int MaxPercentageStorage { get; set; } = 95;

    public IEnumerable<string> NotifierConfigurations { get; set; } = [];
}
