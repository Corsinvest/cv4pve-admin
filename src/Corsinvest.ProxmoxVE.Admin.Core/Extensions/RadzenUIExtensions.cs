/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Linq.Expressions;
using Corsinvest.ProxmoxVE.Admin.Core.Components;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class RadzenUIExtensions
{
    public static void SetRowStyleError<T>(this RowRenderEventArgs<T> args)
        => args.Attributes.Add("style", $"background-color: {Colors.DangerLight};");

    public static void SetRowStyleWarning<T>(this RowRenderEventArgs<T> args)
        => args.Attributes.Add("style", $"background-color: {Colors.WarningDark};");

    public static void Success(this NotificationService notificationService, string summary, string detail = null!)
        => notificationService.Notify(new()
        {
            Severity = NotificationSeverity.Success,
            Summary = summary,
            Detail = detail ?? string.Empty,
        });

    public static void Info(this NotificationService notificationService, string summary, string detail = null!)
        => notificationService.Notify(new()
        {
            Severity = NotificationSeverity.Info,
            Summary = summary,
            Detail = detail ?? string.Empty
        });

    public static void Error(this NotificationService notificationService, string summary, string detail = null!)
        => notificationService.Notify(new()
        {
            Severity = NotificationSeverity.Error,
            Summary = summary,
            Detail = detail ?? string.Empty
        });

    public static void Warning(this NotificationService notificationService, string summary, string detail = null!)
        => notificationService.Notify(new()
        {
            Severity = NotificationSeverity.Warning,
            Summary = summary,
            Detail = detail ?? string.Empty
        });

    public static async Task BusyAsync(this DialogService dialogService, string message)
        => await dialogService.OpenAsync(string.Empty, _ =>
        {
            return a =>
            {
                a.OpenElement(0, nameof(RadzenRow));
                a.OpenElement(1, nameof(RadzenColumn));
                a.AddAttribute(2, nameof(RadzenColumn.Size), "12");
                a.AddContent(3, message);
                a.CloseElement();
                a.CloseElement();
            };
        }, new()
        {
            ShowTitle = false,
            Style = "min-height:auto;min-width:auto;width:auto",
            CloseDialogOnEsc = false
        });

    public static async Task BusyAsync(this DialogService dialogService, RenderFragment<DialogService> childContent)
        => await dialogService.OpenAsync(string.Empty, childContent, new()
        {
            ShowTitle = false,
            Style = "min-height:auto;min-width:auto;width:auto",
            CloseDialogOnEsc = false
        });

    public static async Task OpenSideLogAsync(this DialogService dialogService, string title, string logs)
        => await dialogService.OpenSideExAsync<LogDialog>(title,
                                                          new() { [nameof(LogDialog.Model)] = logs },
                                                          new()
                                                          {
                                                              CloseDialogOnOverlayClick = true,
                                                              Width = "700px"
                                                          });

    public static async Task<dynamic> OpenSideExAsync<T>(this DialogService dialogService,
                                                         string title,
                                                         Dictionary<string, object> parameters,
                                                         DialogOptions options)
    where T : ComponentBase
    {
        options.CssClass ??= "rz-dialog-side";
        options.Style = "right: 0;";
        options.Width ??= "600px";
        return await dialogService.OpenAsync<T>(title, parameters!, options);
    }

    public static async Task<dynamic> OpenSideEditAsync<TDialog>(this DialogService dialogService,
                                                                 string title,
                                                                 bool isNew,
                                                                 object model,
                                                                 Func<object, bool, Task<bool>> onSubmiting = null!,
                                                                 DialogOptions sideDialogOptions = null!)
        where TDialog : ComponentBase
        => await OpenSideExAsync<EditDialog<TDialog>>(dialogService,
                                                      title,
                                                      new()
                                                      {
                                                          [nameof(EditDialog<>.Model)] = model!,
                                                          [nameof(EditDialog<>.IsNew)] = isNew,
                                                          [nameof(EditDialog<>.OnSubmiting)] = onSubmiting ?? ((_, __) => Task.FromResult(true))
                                                      },
                                                      sideDialogOptions ?? new()
                                                      {
                                                          CloseDialogOnOverlayClick = true,
                                                          Width = "600px"
                                                      });

    public static async Task<ResultLoadData<TResult>> LoadDataAsync<TSource, TResult>(this RadzenDataGrid<TResult> grid,
                                                                                      IQueryable<TSource> query,
                                                                                      LoadDataArgs args,
                                                                                      Expression<Func<TSource, TResult>> selector,
                                                                                      string? lastFilter)
       where TResult : notnull
       where TSource : class
        => await query.LoadDataAsync(args, grid, selector, lastFilter);

    public static async Task<string?> PromptAsync(this DialogService dialogService,
                                                   string title,
                                                   string label,
                                                   string defaultValue = "")
    {
        var result = await dialogService.OpenAsync<PromptDialog>(title,
                                                                 new()
                                                                 {
                                                                     [nameof(PromptDialog.Label)] = label,
                                                                     [nameof(PromptDialog.DefaultValue)] = defaultValue
                                                                 },
                                                                 new DialogOptions { Width = "400px" });

        return result is string value ? value : null;
    }

    public static async Task<bool> ConfirmAsync(this DialogService dialogService,
                                                string message,
                                                string title,
                                                bool danger,
                                                string yesText = "yes",
                                                string noText = "no")
        => await dialogService.Confirm(message,
                                       title,
                                       new ConfirmOptions
                                       {
                                           CssClass = danger ? "cv-confirm-danger-dialog" : string.Empty,
                                           OkButtonText = yesText,
                                           CancelButtonText = noText
                                       }) ?? false;
}
