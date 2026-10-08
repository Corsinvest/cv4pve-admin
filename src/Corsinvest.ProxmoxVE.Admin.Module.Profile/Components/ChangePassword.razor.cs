/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Microsoft.AspNetCore.Components;

namespace Corsinvest.ProxmoxVE.Admin.Module.Profile.Components;

public partial class ChangePassword(UserManager<ApplicationUser> userManager,
                                    ICurrentUserService currentUserService,
                                    IAuditService auditService,
                                    DialogService dialogService,
                                    NavigationManager navigationManager,
                                    NotificationService notificationService)
{
    private string? Error { get; set; }
    private InputModel Model { get; set; } = new();
    private bool ShowPasswords { get; set; }

    private class InputModel
    {
        [Required, DataType(DataType.Password)]
        public string OldPassword { get; set; } = default!;

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; } = default!;

        [DataType(DataType.Password), Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = default!;
    }

    private async Task SubmitAsync()
    {
        Error = null;

        var user = (await userManager.FindByIdAsync(currentUserService.UserId))!;
        var result = await userManager.ChangePasswordAsync(user, Model.OldPassword, Model.NewPassword);
        if (result.Succeeded)
        {
            await auditService.LogAsync("ChangePassword", true, $"User '{user.UserName}' changed the password");

            // Leave nothing behind on screen: clear the fields and put any revealed password
            // back under the dots.
            Model = new();
            ShowPasswords = false;

            await dialogService.Alert(L["Your password has been changed"], L["Password"]);
            navigationManager.RefreshSignIn();
        }
        else
        {
            Error = result.Errors.Select(a => a.Description).JoinAsString(",");
            await auditService.LogAsync("ChangePassword", false, $"User '{user.UserName}': {Error}");
            notificationService.Error("Error", Error);
            return;
        }
    }
}
