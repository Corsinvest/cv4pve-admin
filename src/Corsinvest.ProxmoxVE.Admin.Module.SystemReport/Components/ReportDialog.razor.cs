/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Components;

public partial class ReportDialog(NotificationService notificationService) : IModelParameter<JobResult>
{
    [Parameter] public JobResult Model { get; set; } = default!;
    [CascadingParameter(Name = nameof(Mode))] public EditDialogMode Mode { get; set; }

    private bool ReadOnly => Mode == EditDialogMode.ReadOnly;
    private Report.Settings S => Model.Settings;

    // Since and Until of the report settings are dates without time, edited as one range.
    private static DateRange? ToRange(DateOnly? since, DateOnly? until)
        => since is null && until is null
            ? null
            : new(since?.ToDateTime(TimeOnly.MinValue), until?.ToDateTime(TimeOnly.MinValue));

    private static DateOnly? ToDateOnly(DateTime? value) => value is { } date ? DateOnly.FromDateTime(date) : null;

    private void ApplyPreset(Report.Settings preset, string presetName)
    {
        Model.Settings = preset;
        notificationService.Info(L[$"Settings set to {presetName} mode"]);
        StateHasChanged();
    }
}
