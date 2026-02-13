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
    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? Code { get; set; }

    private string? Message { get; set; }

    private sealed class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        public string Code { get; set; } = default!;
    }

    protected override void OnInitialized()
    {
        if (Code is null) { navigationManager.NavigateTo("/InvalidPasswordReset"); }
        Input.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code!));
    }

    private async Task ResetAsync()
    {
        var user = await UserManager.FindByEmailAsync(Input.Email);
        if (user is null) { navigationManager.NavigateTo("/ResetPasswordConfirmation"); }

        var result = await UserManager.ResetPasswordAsync(user!, Input.Code, Input.Password);
        if (result.Succeeded) { navigationManager.NavigateTo("/ResetPasswordConfirmation"); }

        Message = result.Errors is null
                    ? null
                    : string.Join(", ", result.Errors.Select(a => a.Description));
    }
}
