/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Modularity;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class PveAdminHelper
{
    public static string UrlShopSubscription { get; } = "https://shop.corsinvest.it";

    static PveAdminHelper()
    {
        ModuleCategories = new()
        {
            [AdminModuleCategory.Control] = new ModuleCategory("Control", Icons.Material.Filled.Gamepad, 0),
            [AdminModuleCategory.Health] = new ModuleCategory("Health", Icons.Material.Filled.MonitorHeart, 0),
            [AdminModuleCategory.Management] = new ModuleCategory("Management", Icons.Material.Filled.ManageAccounts, 0),
            [AdminModuleCategory.Safe] = new ModuleCategory("Safe", Icons.Material.Filled.AccountBalanceWallet, 0),
            [AdminModuleCategory.System] = new ModuleCategory(IModularityService.AdministrationCategoryName, Icons.Material.Filled.SettingsApplications, 0),
            [AdminModuleCategory.Utilities] = new ModuleCategory("Utilities", Icons.Material.Filled.Handyman, 0),
            [AdminModuleCategory.Develop] = new ModuleCategory("Develop", Icons.Material.Filled.Code, 0),
            [AdminModuleCategory.General] = new ModuleCategory(IModularityService.GeneralCategoryName, Icons.Material.Filled.Folder, 0),
            [AdminModuleCategory.Info] = new ModuleCategory("Info", Icons.Material.Filled.Info, 0),
            [AdminModuleCategory.Support] = new ModuleCategory("Support", Icons.Material.Filled.Support, 9999),
        };
    }

    public static string SvgIconApp { get; } = "<g transform=\"matrix(0.004684, 0, 0, -0.004684, 0.008197, 23.984467)\" stroke=\"none\" data-bx-origin=\"0 5.206446\"><path d=\"M404 5105 c-191 -41 -349 -201 -390 -394 -21 -99 -21 -4203 0 -4302 42 -195 200 -353 395 -395 99 -21 4203 -21 4302 0 195 42 353 200 395 395 21 99 21 4203 0 4302 -42 195 -200 353 -395 395 -95 20 -4214 19 -4307 -1z m2970 -1980 c37 -18 10 26 278 -439 120 -209 223 -384 227 -388 3 -4 112 176 241 400 214 370 238 409 272 425 49 24 101 9 129 -37 18 -30 19 -56 19 -558 l0 -528 -105 0 -105 0 -2 326 -3 326 -175 -308 c-96 -169 -183 -318 -195 -332 -13 -15 -35 -25 -62 -29 -38 -5 -46 -2 -75 28 -19 19 -110 168 -203 332 l-170 299 -3 -321 -2 -321 -105 0 -105 0 0 533 c0 588 -3 564 64 593 40 17 42 17 80 -1z m-1728 -39 l34 -34 0 -526 0 -526 -105 0 -105 0 0 135 0 135 -350 0 -349 0 -3 -132 -3 -133 -102 -3 -103 -3 0 320 c0 193 4 341 11 373 20 95 76 197 148 268 77 76 141 113 243 140 63 16 111 19 362 19 l288 1 34 -34z m1026 23 c158 -34 291 -155 339 -308 24 -80 34 -262 20 -381 -21 -177 -106 -302 -258 -377 l-77 -38 -350 -3 c-193 -2 -361 0 -374 3 -12 3 -31 20 -42 38 -19 31 -20 53 -20 521 l0 488 34 34 34 34 321 0 c199 0 340 -4 373 -11z\"></path><path d=\"M989 2887 c-81 -28 -132 -65 -170 -125 -39 -62 -49 -102 -49 -199 l0 -83 350 0 350 0 0 215 0 215 -207 0 c-189 0 -214 -3 -274 -23z\"></path><path d=\"M2120 2554 l0 -356 258 4 c292 4 315 9 383 85 56 62 64 96 64 278 0 152 -1 162 -26 210 -29 56 -91 109 -147 125 -21 5 -145 10 -284 10 l-248 0 0 -356z\"></path></g>";
    public static string SvgIconPveLogoBlack { get; } = "<path d=\"M4.928 1.825c-1.09.553-1.09.64-.07 1.78 5.655 6.295 7.004 7.782 7.107 7.782.139.017 7.971-8.542 8.058-8.801.034-.07-.208-.312-.519-.536-.415-.312-.864-.433-1.712-.467-1.59-.104-2.144.242-4.115 2.455-.899 1.003-1.66 1.833-1.66 1.833-.017 0-.76-.813-1.642-1.798S8.473 2.1 8.127 1.91c-.796-.45-2.421-.484-3.2-.086zM1.297 4.367C.45 4.695 0 5.007 0 5.248c0 .121 1.331 1.678 2.94 3.459 1.625 1.78 2.939 3.268 2.939 3.302 0 .035-1.331 1.522-2.94 3.303C1.314 17.11.017 18.683.035 18.822c.086.467 1.504 1.055 2.541 1.055 1.678-.018 2.058-.312 5.603-4.202 1.78-1.954 3.233-3.614 3.233-3.666 0-.069-1.435-1.694-3.199-3.63-2.3-2.508-3.423-3.632-3.96-3.874-.812-.398-2.126-.467-2.956-.138zm18.467.12c-.502.26-1.764 1.505-3.943 3.891-1.763 1.937-3.199 3.562-3.199 3.631 0 .07 1.453 1.712 3.234 3.666 3.544 3.89 3.925 4.184 5.602 4.202 1.038 0 2.455-.588 2.542-1.055.017-.156-1.28-1.712-2.905-3.493-1.608-1.78-2.94-3.285-2.94-3.32 0-.034 1.332-1.539 2.94-3.32C22.72 6.91 24.017 5.352 24 5.214c-.087-.45-1.366-.968-2.473-1.038-.795-.034-1.21.035-1.763.312zM7.954 16.973c-2.144 2.369-3.908 4.374-3.943 4.46-.034.07.208.312.52.537.414.311.864.432 1.711.467 1.574.103 2.161-.26 4.15-2.508.864-.968 1.608-1.78 1.625-1.78s.761.812 1.643 1.798c2.023 2.248 2.559 2.576 4.132 2.49.848-.035 1.297-.156 1.712-.467.311-.225.553-.467.519-.536-.087-.26-7.92-8.819-8.058-8.801-.069 0-1.867 1.954-4.011 4.34z\"/>";
    public static Version MinimalVersion { get; } = new Version(6, 4);

    public static Dictionary<AdminModuleCategory, ModuleCategory> ModuleCategories { get; } = default!;

    public static async Task<string> GenerateWhoUsing(PveClient client, AdminOptions adminOptions)
    {
        var resources = (await client.GetResources(ClusterResourceType.All))
                            .CalculateHostUsage();
        var lxc = resources.Where(a => a.ResourceType == ClusterResourceType.Vm && a.VmType == VmType.Lxc).Count();
        var qemu = resources.Where(a => a.ResourceType == ClusterResourceType.Vm && a.VmType == VmType.Qemu).Count();

        var nodes = resources.Where(a => a.ResourceType == ClusterResourceType.Node && a.IsOnline);
        var allStorage = resources.Where(a => a.ResourceType == ClusterResourceType.Storage && a.IsAvailable);

        var storages = allStorage.Where(a => !a.Shared).ToList();
        storages.AddRange(allStorage.Where(a => a.Shared).DistinctBy(a => a.Storage));

        return @$"Proxmox VE Version: {(await client.Version.Get()).Version}
Host Number: {nodes.Count()}
CPUs: {nodes.Sum(a => a.CpuSize)}
Memory: {FormatHelper.FromBytes(nodes.Sum(a => a.MemorySize))}
Storage: {FormatHelper.FromBytes(storages.Sum(a => a.DiskSize))}
VM/CT Number: {qemu}/{lxc}
Company: {adminOptions.Company}
Only post your data/question here, please don't comment or ask questions.
";
    }

    public static async Task<string> GeClusterInfo(PveClient client, Configurations.ClusterOptions clusterOptions)
    {
        var rows = new List<IEnumerable<string>>();

        var status = await client.Cluster.Status.Get();

        foreach (var item in status.Where(a => !string.IsNullOrWhiteSpace(a.IpAddress)).OrderBy(a => a.Name))
        {
            var node = clusterOptions.GetNodeOptions(item.IpAddress, item.Name);

            var version = item.IsOnline
                            ? (await client.Nodes[item.Name].Version.Get())?.Version
                            : "";

            rows.Add(new string[] { node?.ServerId, version, item.Name, item.IpAddress, node?.SubscriptionId });
        }

        return TableGenerator.ToText(new[] { "Server Id", "PVE Version", "Name", "IpAddress", "Subscription Id" }, rows);
    }
}