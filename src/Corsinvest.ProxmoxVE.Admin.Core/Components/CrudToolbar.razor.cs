using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class CrudToolbar(ContextMenuService contextMenuService)
{
    [CascadingParameter(Name = nameof(IClusterName.ClusterName))] public string? CascadedClusterName { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnAdd { get; set; } = default!;
    [Parameter] public EventCallback<MouseEventArgs> OnDelete { get; set; } = default!;
    [Parameter] public bool AddDisabled { get; set; }
    [Parameter] public bool DeleteDisabled { get; set; }
    [Parameter] public bool Disabled { get; set; } = default!;
    [Parameter] public EventCallback<MouseEventArgs> OnRefresh { get; set; } = default!;
    [Parameter] public EventCallback<MouseEventArgs> OnExport { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? TemplateBefore { get; set; }
    [Parameter] public RenderFragment? TemplateAfter { get; set; }
    [Parameter] public bool InRefresh { get; set; }
    [Parameter] public bool AllowMoreMenu { get; set; }
    [Parameter] public RenderFragment? TemplateMoreMenu { get; set; }
    [Parameter] public PermissionsRead? Permissions { get; set; }

    [Parameter] public Permission? PermissionDelete { get; set; }
    [Parameter] public Permission? PermissionCreate { get; set; }

    [Parameter] public string? PermissionClusterName { get; set; }

    private bool CanCreate { get; set; } = true;
    private bool CanDelete { get; set; } = true;
    private bool CanExport { get; set; } = true;

    private async Task Refresh()
    {
        InRefresh = true;
        await OnRefresh.InvokeAsync();
        InRefresh = false;
    }

    protected override async Task OnInitializedAsync()
    {
        var permissionClusterName = PermissionClusterName ?? CascadedClusterName!;
        if (!string.IsNullOrEmpty(permissionClusterName))
        {
            if (Permissions != null)
            {
                CanExport = await HasPermissionAsync(permissionClusterName, Permissions.Export);
                if (Permissions is PermissionsCrud crud)
                {
                    CanCreate = await HasPermissionAsync(permissionClusterName, crud.Create);
                    CanDelete = await HasPermissionAsync(permissionClusterName, crud.Delete);
                }
            }
            else
            {
                if (PermissionDelete != null)
                {
                    CanDelete = await HasPermissionAsync(permissionClusterName, PermissionDelete);
                }

                if (PermissionCreate != null)
                {
                    CanCreate = await HasPermissionAsync(permissionClusterName, PermissionCreate);
                }
            }
        }
    }
}
