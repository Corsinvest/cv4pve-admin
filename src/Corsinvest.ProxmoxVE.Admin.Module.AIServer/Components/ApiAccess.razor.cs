/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.AIServer.Components;

public partial class ApiAccess(IModuleService moduleService,
                               ISettingsService settingsService,
                               EventNotificationService eventNotificationService) : IDisposable
{
    protected bool McpEnabled { get; set; }
    protected bool McpAuthFailed { get; set; }
    private IEnumerable<Permission> ToolPermissions { get; set; } = [];

    protected override void OnInitialized()
    {
        var module = moduleService.Get<Module>()!;
        module.SettingsUpdated += SettingsUpdated;
        eventNotificationService.Subscribe<McpAuthFailedNotification>(HandleMcpAuthFailedAsync);

        ToolPermissions = [.. module.AllRoles
                                    .Where(r => r.Key == Module.RoleName)
                                    .SelectMany(r => r.Permissions)];
        LoadSettings();
    }

    private async Task HandleMcpAuthFailedAsync(McpAuthFailedNotification _)
    {
        McpAuthFailed = true;
        await InvokeAsync(StateHasChanged);
    }

    private void LoadSettings()
    {
        var settings = settingsService.GetForModule<Module, Settings>(ApplicationHelper.AllClusterName);
        McpEnabled = settings.Enabled;
        StateHasChanged();
    }

    private void SettingsUpdated(object? sender, EventArgs e) => LoadSettings();

    public void Dispose()
    {
        moduleService.Get<Module>()!.SettingsUpdated -= SettingsUpdated;
        eventNotificationService.Unsubscribe<McpAuthFailedNotification>(HandleMcpAuthFailedAsync);
    }
}
