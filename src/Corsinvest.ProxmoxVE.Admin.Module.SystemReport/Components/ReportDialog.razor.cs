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

    private void ApplyPreset(Report.Settings preset, string presetName)
    {
        Model.Settings = preset;
        notificationService.Info(L[$"Settings set to {presetName} mode"]);
        StateHasChanged();
    }
}
