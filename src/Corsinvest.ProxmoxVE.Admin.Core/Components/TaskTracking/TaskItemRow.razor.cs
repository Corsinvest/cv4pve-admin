/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;
using Humanizer;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.TaskTracking;

public partial class TaskItemRow(ITaskTrackerService TaskTracker,
                                 DialogService DialogService,
                                 NavigationManager NavigationManager)
{
    [Parameter] public TaskItemInfo Item { get; set; } = default!;
    [Parameter] public EventCallback OnAfterAction { get; set; }

    private bool CanStop { get; set; }

    protected override async Task OnInitializedAsync()
        => CanStop = await HasPermissionAsync(Permissions.Stop);

    private string Icon => Item.Status switch
    {
        TaskItemStatus.Running => "sync",
        TaskItemStatus.Completed => "check_circle",
        TaskItemStatus.Failed => "dangerous",
        TaskItemStatus.Cancelled => "cancel",
        TaskItemStatus.Abandoned => "unpublished",
        _ => "help_outline"
    };

    private string Color => Item.Status switch
    {
        TaskItemStatus.Running => Colors.Warning,
        TaskItemStatus.Completed => Colors.Success,
        TaskItemStatus.Failed => Colors.Danger,
        TaskItemStatus.Cancelled => Colors.Secondary,
        TaskItemStatus.Abandoned => Colors.Warning,
        _ => Colors.Secondary
    };

    private string Elapsed => (DateTime.UtcNow - Item.StartedAt).Humanize();

    private async Task CancelTaskAsync()
    {
        await TaskTracker.CancelAsync(Item.Id);
        await OnAfterAction.InvokeAsync();
    }

    private async Task ShowDetailsAsync()
    {
        var logs = await TaskTracker.GetLogsAsync(Item.Id);
        await DialogService.OpenSideLogAsync(Item.Title, string.Join(Environment.NewLine, logs));
    }
}
