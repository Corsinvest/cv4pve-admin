using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.Resources;

public class Module : ModuleBase
{
    public Module()
    {
        Keywords = "system,status,monitoring,cluster,nodes,vm,storage,health,realtime";
        ModuleType = ModuleType.Application;
        Name = "Resources";
        Description = "Monitor real-time cluster and resource status";
        Category = Categories.Health;
        Slug = "resources";

        NavBar =
        [
            new(this,"Overview", string.Empty)
            {
                Render = new(typeof(Components.Overview)),
                Icon = PveAdminUIHelper.Icons.Overview
            },
            new(this,"Cluster")
            {
                Render = new(typeof(Components.Clusters)),
                Icon = PveAdminUIHelper.Icons.Cluster
            },
            new(this,"Nodes")
            {
                Render = new(typeof(Components.Nodes)),
                Icon = PveAdminUIHelper.Icons.Node
            },
            new(this,"VMs")
            {
                Render = new(typeof(Components.Vms)),
                Icon = PveAdminUIHelper.Icons.Vm
            },
            new(this,"Storages")
            {
                Render = new(typeof(Components.Storages)),
                Icon = PveAdminUIHelper.Icons.Storage
            },
            new(this,"Snapshots")
            {
                Render = new(typeof(Components.Snapshots)),
                Icon = PveAdminUIHelper.Icons.Snapshot
            }
        ];

        Link = new(this, Name, string.Empty)
        {
            Icon = "verified",
            Render = NavBar.ToList()[0].Render
        };

        Widgets =
        [
           new(this,"Resources Status")
           {
               Description = "Show resources status",
               RenderInfo = new(typeof(Components.Widgets.Status.Render)),
               RenderSettingsInfo = new(typeof(Components.Widgets.Status.Settings),
                                        typeof(Components.Widgets.Status.RenderSettings)),
               Height = 3,
               Width = 3,
           },
           new(this,"Resources Usage")
           {
               Description = "Show resources usage",
               RenderInfo = new(typeof(Components.Widgets.ResourcesUsage.Render)),
               RenderSettingsInfo = new(typeof(Components.Widgets.ResourcesUsage.Settings),
                                        typeof(Components.Widgets.ResourcesUsage.RenderSettings)),
               Height = 7,
               Width = 9,
           },
           new(this,"Cluster Usage Gauge")
           {
               Description = "Show cluster usage",
               RenderInfo = new(typeof(Components.Widgets.ClusterUsage.Gauge)),
               Height = 4,
               Width = 4,
           },
           new(this,"Cluster Usage Grid")
           {
               Description = "Show cluster usage",
               RenderInfo = new(typeof(Components.Widgets.ClusterUsage.Grid)),
               Height = 4,
               Width = 5,
           },
           new(this,"Clusters Maps")
           {
               Description = "Show clusters maps",
               RenderInfo = new(typeof(Components.Widgets.Maps.Render)),
               Height = 7,
               Width = 9,
           },
           new(this,"VM/CT Locked")
           {
               Description = "VM/CT Locked",
               RenderInfo = new(typeof(Components.Widgets.VmsLocked)),
               Height = 3,
               Width = 3,
           },
           new(this,"Nodes Status")
           {
               Description = "Nodes Status",
               RenderInfo = new(typeof(Components.Widgets.NodesStatus)),
               Height = 3,
               Width = 3,
           }
        ];
    }

    protected override string PermissionBaseKey { get; } = "Status";
}
