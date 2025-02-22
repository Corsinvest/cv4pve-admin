/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Options;
using Corsinvest.AppHero.Core.Service;
using Corsinvest.AppHero.Core.UI;
using MudExtensions;
using MudExtensions.Enums;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.Dialogs;

public partial class DialogFastConfig
{
    [Inject] private IPveClientService PveClientService { get; set; } = default!;
    [Inject] private IWritableOptionsService<AdminOptions> WritableOptionsService { get; set; } = default!;
    [Inject] private IOptionsSnapshot<AdminOptions> AdminOptions { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IBrowserService BrowserService { get; set; } = default!;

    private MudStepper? RefStepper { get; set; } = default!;
    private MudForm? RefForm { get; set; } = default!;

    public ClusterOptions ClusterOptions => AdminOptions.Value.Clusters[0];

    protected override void OnInitialized()
    {
        if (AdminOptions.Value.Clusters.Count == 0) { AdminOptions.Value.Clusters.Add(new()); }
    }

    private async Task<bool> PreventStepChangeAsync(StepChangeDirection direction, int activeIndex)
    {
        var ret = false;

        if (direction == StepChangeDirection.Forward)
        {
            switch (RefStepper!.GetActiveIndex())
            {
                case 0:
                    ret = ClusterOptions.UseApiToken
                            ? ClusterOptions.ApiToken.IsNullOrEmptyOrWhiteSpace()
                            : ClusterOptions.ApiCredential.Username.IsNullOrEmptyOrWhiteSpace();

                    if (ret) { UINotifier.Show(L[(ClusterOptions.UseApiToken ? "Api Token" : "Credential") + " is required!"], 
                               UINotifierSeverity.Error); }
                    break;

                case 1:
                    ret = ClusterOptions.SshCredential.Username.IsNullOrEmptyOrWhiteSpace();
                    if (ret) { UINotifier.Show(L["Credential is required!"], UINotifierSeverity.Error); }
                    break;

                case 2:
                    try
                    {
                        //check pve login
                        var client = await PveClientService.GetClientAsync(ClusterOptions);
                        ret = client == null;
                        if (!ret && !await PveClientService.CheckIsValidVersionAsync(client!))
                        {
                            UINotifier.Show(L["Proxmox VE version not valid! Required {0}", PveAdminHelper.MinimalVersion],
                                            UINotifierSeverity.Error);
                        }
                        else
                        {
                            //get cluster name
                            if (client != null)
                            {
                                var info = await client.GetClusterInfoAsync();
                                ClusterOptions.Name = info.Name;
                                ClusterOptions.Type = info.Type;
                            }

                            UINotifier.Show(!ret,
                                            L["Successful connection!"],
                                            L["Problem connection!"],
                                            L["Test connection Proxmox VE cluster!"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        UINotifier.Show(ex.Message, UINotifierSeverity.Error);
                        ret = true;
                    }
                    break;

                case 3:
                    await RefForm!.Validate();
                    ret = !RefForm.IsValid;
                    break;

                default: break;
            }
        }

        WritableOptionsService.Update(AdminOptions.Value);

        return ret;
    }

    private async Task ResultStepClick() => await BrowserService.OpenAsync(NavigationManager.Uri.ToString(), "_self");
}
