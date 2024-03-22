/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.ClusterStatus.Components;
using Corsinvest.ProxmoxVE.Admin.ClusterStatus.Components.WidgetCluster;
using Corsinvest.ProxmoxVE.Admin.ClusterStatus.Components.WidgetInfo;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Corsinvest.ProxmoxVE.Admin.ClusterStatus;

public class Module : PveAdminModuleBase, IForceLoadModule
{
    public Module()
    {
        Authors = "Corsinvest Srl";
        Company = "Corsinvest Srl";
        Keywords = "System";
        Description = "Cluster Status";
        InfoText = "Monitor the current status of your Proxmox VE cluster";
        SetCategory(AdminModuleCategory.Health);

        Link = new ModuleLink(this, Description)
        {
            Icon = Icons.Material.Filled.Verified,
            Render = typeof(RenderIndex)
        };

        Roles = [new("", "", Permissions.DataGrid.Data.Permissions)];

        Widgets =
        [
            //new ModuleWidget(this,"Status")
            //{
            //    Render= typeof(RenderWidget1),
            //},

            #region Info
            new ModuleWidget(this,"00ClusterStatus")
            {
                Render= typeof(RenderWidgetInfoClusterStatus),
                ShowDefaultHeader = false,
                Class="mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-4 mud-grid-item-md-2 mud-grid-item-lg-2"
            },

            new ModuleWidget(this,"01NodesOnlines")
            {
                Render= typeof(RenderWidgetInfoNodesOnlines),
                ShowDefaultHeader = false,
                Class="mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-4 mud-grid-item-md-2 mud-grid-item-lg-2"
            },

            new ModuleWidget(this,"02NodesUpdate")
            {
                Render= typeof(RenderWidgetInfoNodesUpdates),
                ShowDefaultHeader = false,
                Class="mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-4 mud-grid-item-md-2 mud-grid-item-lg-2"
            },

            new ModuleWidget(this,"03NodesTasks")
            {
                Render= typeof(RenderWidgetInfoNodesTasks),
                ShowDefaultHeader = false,
                Class="mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-4 mud-grid-item-md-2 mud-grid-item-lg-2"
            },

            new ModuleWidget(this,"04Bacukps")
            {
                Render= typeof(RenderWidgetInfoBackups),
                ShowDefaultHeader = false,
                Class="mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-4 mud-grid-item-md-2 mud-grid-item-lg-2"
            },
            #endregion

            #region Cluster Status Cpu/Memory/Storage
            new ModuleWidget(this,"00Cpu")
            {
                GroupName=" Cluster Resources",
                Render= typeof(RenderWidgetCpu),
                ShowDefaultHeader = false,
                Class="mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-6 mud-grid-item-md-4 mud-grid-item-lg-4"
            },

            new ModuleWidget(this,"01Memory")
            {
                GroupName=" Cluster Resources",
                Render= typeof(RenderWidgetMemory),
                ShowDefaultHeader=false,
                Class="mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-6 mud-grid-item-md-4 mud-grid-item-lg-4"
            },

            new ModuleWidget(this,"02Storage")
            {
                GroupName =" Cluster Resources",
                Render = typeof(RenderWidgetStorage),
                ShowDefaultHeader = false,
                Class = "mud-grid-item mud-grid-item-xs-12 mud-grid-item-sm-6 mud-grid-item-md-4 mud-grid-item-lg-4"
            },
            #endregion
        ];

        UrlHelp += "#chapter_module_cluster_status";
    }

    public override void ConfigureServices(IServiceCollection services, IConfiguration config)
        => AddOptions<Options, RenderOptions>(services, config);

    public class Permissions
    {
        public class DataGrid
        {
            public static PermissionsRead Data { get; } = new($"{typeof(Module).FullName}.{nameof(DataGrid)}.{nameof(Data)}");
        }
    }
}