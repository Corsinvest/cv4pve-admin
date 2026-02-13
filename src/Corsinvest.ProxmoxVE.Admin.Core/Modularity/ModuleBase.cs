/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Microsoft.AspNetCore.Builder;

namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public abstract class ModuleBase
{
    public event EventHandler? SettingsUpdated;

    public string Type => GetType().FullName!;
    public string PathData => Path.Combine(ApplicationHelper.ModulesPath, Type);

    private string _slug = default!;
    public string Slug
    {
        //if not set use Class
        get => string.IsNullOrWhiteSpace(_slug) || string.IsNullOrEmpty(_slug)
                    ? Type.Replace(".", string.Empty)
                    : _slug;

        set => _slug = value;
    }

    public ModuleLinkBase? Link { get; set; }
    public ModuleLinkPosition LinkPosition { get; set; } = ModuleLinkPosition.NavBar;
    public IEnumerable<ModuleLinkBase> NavBar { get; set; } = [];
    protected IEnumerable<Permission> NavBarPermissions => NavBar.Traverse(a => a.Child).Select(a => a.Permission);
    public IEnumerable<ModuleWidget> Widgets { get; set; } = [];

    public string BaseUrl => $"{ApplicationHelper.ModuleComponentUrl}/{Slug}";

    private string? _icon;
    public string? Icon
    {
        get => string.IsNullOrWhiteSpace(_icon)
                    ? Link?.Icon
                    : _icon;

        set => _icon = value;
    }

    public string Name { get; init; } = default!;

    public string? _description;
    public string Description
    {
        get => _description ?? Name;
        set => _description = value;
    }

    public Category? Category { get; init; }
    public ModuleType ModuleType { get; init; } = ModuleType.Application;
    public string FullInfo => $"{Type}@{Version}";
    public string Keywords { get; init; } = string.Empty;
    public string Authors { get; init; } = "Corsinvest Srl";
    public string Company { get; init; } = "Corsinvest Srl";
    public ClusterScope Scope { get; set; } = ClusterScope.Single;
    public virtual Version Version => GetType().Assembly.GetName().Version!;
    protected internal virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration) { }
    protected internal virtual void Map(WebApplication app) { }
    protected internal virtual Task RunAsync(IServiceScope scope) => Task.CompletedTask;
    protected internal virtual Task InitializeAsync(IServiceScope scope) => Task.CompletedTask;
    public virtual Task FixAsync(IServiceScope scope) => Task.CompletedTask;
    public virtual Task DatabaseMaintenanceAsync(IServiceScope scope, DatabaseMaintenanceOperation operation) => Task.CompletedTask;

    internal async Task RefreshSettingsEventAsync(IServiceScope scope)
    {
        await RefreshSettingsAsync(scope);
        SettingsUpdated?.Invoke(this, EventArgs.Empty);
    }

    protected internal virtual Task RefreshSettingsAsync(IServiceScope scope) => Task.CompletedTask;

    protected IServiceCollection AddSettings<TSetting, TRenderSettings>(IServiceCollection services)
        where TSetting : class, new()
        where TRenderSettings : class //, new()
    {
        RenderSettingsInfo = new(typeof(TSetting), typeof(TRenderSettings));
        return services;
    }

    public RenderSettingsInfo? RenderSettingsInfo { get; private set; }

    public string GetClusterNameForScope(string clusterName)
        => Scope switch
        {
            ClusterScope.Single => clusterName,
            ClusterScope.All => ApplicationHelper.AllClusterName,
            _ => throw new InvalidEnumArgumentException()
        };

    public bool Search(string value)
        => Name.Contains(value, StringComparison.InvariantCultureIgnoreCase)
            || Description.Contains(value, StringComparison.InvariantCultureIgnoreCase)
            || Keywords.Split(",").Any(a => a.Contains(value, StringComparison.InvariantCultureIgnoreCase));

    #region Authorization
    public async Task<bool> HasPermissionAsync(IPermissionService permissionService, string clusterName, Permission permission)
        => await permissionService.HasAsync(GetClusterNameForScope(clusterName), permission);

    public async Task<bool> HasPermissionLinkAsync(IPermissionService permissionService, string clusterName)
        => await permissionService.HasAsync(GetClusterNameForScope(clusterName), Link?.Permission!);

    public async Task<bool> HasPermissionEditorSettingsAsync(IPermissionService permissionService, string clusterName)
        => await permissionService.HasAsync(GetClusterNameForScope(clusterName), PermissionEditSettings);

    protected IEnumerable<Role> Roles { get; set; } = [];

    protected abstract string PermissionBaseKey { get; }

    private string? _permissionLinkBaseKey;
    public string PermissionLinkBaseKey => _permissionLinkBaseKey ??= $"{PermissionBaseKey}.Link";

    private string? _permissionWidgetBaseKey;
    public string PermissionWidgetBaseKey => _permissionWidgetBaseKey ??= $"{PermissionBaseKey}.Widget";

    private Role? _roleAdmin;
    public Role RoleAdmin
        => _roleAdmin ??= new($"{PermissionBaseKey}.{RoleConstants.AdministratorRole}",
                              $"Admin for module {Name}",
                              false,
                              true,
                              GetAllPermissions());

    private List<Permission> GetAllPermissions()
    {
        var permissions = new List<Permission>();

        if (RenderSettingsInfo != null) { permissions.Add(PermissionEditSettings); }
        if (Link != null) { permissions.Add(Link.Permission!); }

        //navbar
        permissions.AddRange(NavBar.Select(a => new[] { a }.Union(a.Child.Traverse(a => a.Child)))
                                   .SelectMany(a => a)
                                   .Where(a => a.Permission != null)
                                   .Select(a => a.Permission!));

        //widgets
        permissions.AddRange(Widgets.Select(a => a.Permission));

        permissions.AddRange(Roles.SelectMany(a => a.Permissions));

        return [.. permissions.DistinctBy(a => a.Key)];
    }

    private Permission? _permissionEditSettings;
    public Permission PermissionEditSettings => _permissionEditSettings ??= new(PermissionBaseKey, "EditOptions", "Edit Settings");
    public IEnumerable<Role> AllRoles => new[] { RoleAdmin }.Union(Roles).Where(a => !string.IsNullOrWhiteSpace(a.Key));
    #endregion

    protected void AddAttribute(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key)) { throw new ArgumentException("Key cannot be null or empty.", nameof(key)); }
        _attributes[key] = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
    }

    private readonly Dictionary<string, object> _attributes = [];
    public IReadOnlyDictionary<string, object> Attributes => _attributes.AsReadOnly();

    //protected void InitializeJob<TJob>(IServiceScope scope, Func<TJob, string, Task> methodCall)
    //{
    //    var jobService = scope.GetJobService();
    //    var settingsService = scope.GetSettingsService();

    //    foreach (var item in settingsService.GetClustersSettings().Select(a => a.Name))
    //    {
    //        var settings = settingsService.GetForModule(GetType(), RenderSettingsInfo!.Type, item);
    //        if (settings is JobScheduleBase job)
    //        {
    //            jobService.ScheduleOrRemove(methodCall(j, item),
    //                                        job.CronExpression,
    //                                        job.Enabled,
    //                                        item);
    //        }
    //    }
    //}
}
