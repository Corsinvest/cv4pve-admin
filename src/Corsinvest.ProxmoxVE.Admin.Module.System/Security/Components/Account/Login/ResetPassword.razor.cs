/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components.Account.Login;

public partial class ResetPassword(NavigationManager navigationManager,
                                   UserManager<ApplicationUser> UserManager)
{
    private InputModel Input { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? UserId { get; set; }

    [SupplyParameterFromQuery]
    private string? Code { get; set; }

    private string? Message { get; set; }
    private bool IsValidLink { get; set; }

    private string? _token;

    private sealed class InputModel
    {
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = default!;
    }

    protected override async Task OnInitializedAsync()
    {
        IsValidLink = await GetUserAsync() != null;
        if (!IsValidLink) { navigationManager.NavigateTo("/InvalidPasswordReset"); }
    }

    private async Task<ApplicationUser?> GetUserAsync()
    {
        if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Code)) { return null; }

        try
        {
            _token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
        }
        catch (FormatException)
        {
            return null;
        }

        var user = await UserManager.FindByIdAsync(UserId);
        if (user == null || user.IsSystem) { return null; }

        return await UserManager.VerifyUserTokenAsync(user,
                                                      UserManager.Options.Tokens.PasswordResetTokenProvider,
                                                      UserManager<ApplicationUser>.ResetPasswordTokenPurpose,
                                                      _token)
                ? user
                : null;
    }

    private async Task ResetAsync()
    {
        if (string.IsNullOrEmpty(Input.Password) || Input.Password != Input.ConfirmPassword)
        {
            Message = L["The password and the confirmation do not match."];
            return;
        }

        var user = await GetUserAsync();
        if (user == null)
        {
            navigationManager.NavigateTo("/InvalidPasswordReset");
            return;
        }

        var result = await UserManager.ResetPasswordAsync(user, _token!, Input.Password);
        if (result.Succeeded)
        {
            navigationManager.NavigateTo("/ResetPasswordConfirmation");
            return;
        }

        Message = string.Join(", ", result.Errors.Select(a => a.Description));
    }
}
