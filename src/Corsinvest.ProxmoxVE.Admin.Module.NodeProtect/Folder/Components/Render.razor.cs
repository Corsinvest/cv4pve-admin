/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net.Mime;
using BlazorDownloadFile;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Models;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Persistence;

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Folder.Components;

public partial class Render(IDbContextFactory<ModuleDbContext> dbContextFactory,
                            IBackgroundJobService backgroundJobService,
                            ISettingsService settingsService,
                            IBlazorDownloadFileService blazorDownloadFileService,
                            DialogService dialogService,
                            NotificationService notificationService,
                            EventNotificationService eventNotificationService) : IRefreshableData,
                                                                                 IClusterName,
                                                                                 IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private IEnumerable<Data> Items { get; set; } = default!;
    private Settings Settings { get; set; } = new();
    private IList<Data> SelectedItems { get; set; } = [];
    private bool InDownload { get; set; }
    private class Data : FolderTaskResult;

    protected override async Task OnInitializedAsync()
    {
        eventNotificationService.Subscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);
        await RefreshDataAsync();
    }

    private async Task HandleDataChangedNotificationAsync(DataChangedNotification notification)
    {
        await RefreshDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    public async Task RefreshDataAsync()
    {
        Settings = settingsService.GetForModule<Module, Settings>(ClusterName);

        await using var db = await dbContextFactory.CreateDbContextAsync();
        Items = await db.FolderTaskResults
                        .FromClusterName(ClusterName)
                        .Select(a => new Data
                        {
                            Id = a.Id,
                            TaskId = a.TaskId,
                            Node = a.Node,
                            FileName = a.FileName,
                            Start = a.Start,
                            End = a.End,
                            Size = a.Size,
                            Status = a.Status
                        })
                        .ToListAsync();
    }

    private async Task ShowLogAsync(Data item)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await dialogService.OpenSideLogAsync(L["Logs"],
                                                    (await db.FolderTaskResults.Where(a => a.Id == item.Id)
                                                                         .Select(a => a.Logs)
                                                                         .FirstOrDefaultAsync())!);
    }

    private async Task DeleteAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Delete selected backup"], true))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            foreach (var item in SelectedItems)
            {
                if (File.Exists(item.GetPath(ClusterName))) { File.Delete(item.GetPath(ClusterName)); }
                if (Directory.GetFiles(item.GetDirectoryWorkJobId(ClusterName)).Length == 0)
                {
                    Directory.Delete(item.GetDirectoryWorkJobId(ClusterName), true);
                }

                await db.FolderTaskResults.DeleteAsync(item.Id);
            }

            await eventNotificationService.PublishAsync(new DataChangedNotification());
            notificationService.Info(L["All data has been removed!"]);
        }
    }

    private async Task DownloadAsync(Data item)
    {
        InDownload = true;
        if (File.Exists(item.GetPath(ClusterName)))
        {
            await using var fs = File.OpenRead(item.GetPath(ClusterName));
            await blazorDownloadFileService.DownloadFile(item.FileName, fs, MediaTypeNames.Application.GZip);
        }
        InDownload = false;
    }

    private void Backup()
    {
        backgroundJobService.Schedule<Job>(a => a.BackupAsync(ClusterName), TimeSpan.FromSeconds(5));
        notificationService.Info(L["Backup started!"]);
    }

    public void Dispose() => eventNotificationService.Unsubscribe<DataChangedNotification>(HandleDataChangedNotificationAsync);

    private void OnRender(DataGridRenderEventArgs<Data> args)
    {
        if (args.FirstRender)
        {
            args.Grid!.Groups.Add(new()
            {
                Title = L["Task Id"],
                Property = nameof(Data.TaskId),
                SortOrder = SortOrder.Descending
            });

            StateHasChanged();
        }
    }
}
