/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components.ClusterConfig;

public partial class WebApiCredentialSettings(IAdminService adminService,
                                       DialogService dialogService,
                                       NotificationService notificationService)
{
    [Parameter, EditorRequired] public ClusterSettings Model { get; set; } = default!;
    [Parameter] public EventCallback ModelChanged { get; set; }

    private async Task CreateApiTokenAsync()
    {
        var result = await dialogService.OpenAsync<CreateApiTokenDialog>(L["Create API Token"]);
        if (result is CreateApiTokenModel dialogResult)
        {
            var clusterClient = adminService.CreateClusterClient(new ClusterSettings
            {
                WebApi = new WebApiCredential
                {
                    AccessType = ClusterAccessType.Credential,
                    Username = dialogResult.Username,
                    Password = dialogResult.Password,
                    Timeout = Model.WebApi.Timeout
                },
                Nodes = Model.Nodes
            });

            try
            {
                var client = await clusterClient.CachedData.GetPveClientAsync();

                // Create dedicated user cv4pve-admin@pve
                var dedicatedUser = "cv4pve-admin@pve";
                var createUserResult = await client.Access.Users.CreateUser(dedicatedUser, comment: "Created by cv4pve-admin", enable: true);
                if (!createUserResult.IsSuccessStatusCode)
                {
                    notificationService.Error(L["Error creating user '{0}'", dedicatedUser], createUserResult.GetError() + "\n" + createUserResult.ReasonPhrase);
                    return;
                }

                // Add role PVEAdmin on dedicated user
                var aclResult = await client.Access.Acl.UpdateAcl("/", "PVEAdmin", propagate: true, users: dedicatedUser);
                if (!aclResult.IsSuccessStatusCode)
                {
                    notificationService.Error(L["Error assigning role to '{0}'", dedicatedUser], aclResult.GetError() + "\n" + aclResult.ReasonPhrase);
                    return;
                }

                // Create token on dedicated user (privsep=0 → inherits PVEAdmin role)
                var tokenResult = await client.Access.Users[dedicatedUser].Token[dialogResult.TokenName].GenerateToken("Created by cv4pve-admin", privsep: false);
                if (tokenResult.IsSuccessStatusCode)
                {
                    var data = (IDictionary<string, object>)tokenResult.ToData();

                    var fullTokenId = data["full-tokenid"].ToString()!;
                    var secret = data["value"].ToString()!;
                    Model.WebApi.ApiToken = $"{fullTokenId}={secret}";
                    Model.WebApi.AccessType = ClusterAccessType.ApiToken;
                    await dialogService.OpenCopyValueAsync(L["API Token created — save it now!"],
                                                           Model.WebApi.ApiToken,
                                                           L["API Token"],
                                                           L["This token will not be shown again. Copy it now: {0}", fullTokenId]);
                }
                else
                {
                    notificationService.Error(L["Error creating token '{0}'", $"{dedicatedUser}!{dialogResult.TokenName}"], tokenResult.GetError() + "\n" + tokenResult.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                notificationService.Error(L["Error"], ex.Message);
            }
        }
    }
}
