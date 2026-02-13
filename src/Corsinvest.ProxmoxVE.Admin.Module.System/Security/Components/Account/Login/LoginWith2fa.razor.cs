/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using ZiggyCreatures.Caching.Fusion;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components.Account.Login;

public partial class LoginWith2fa(NavigationManager navigationManager,
                                  IFusionCache fusionCache)
{
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }
    [SupplyParameterFromQuery] private bool RememberMe { get; set; }
    [SupplyParameterFromQuery] private string Key2FA { get; set; } = default!;

    private InputModel Input { get; set; } = new();
    private RadzenSecurityCode SecurityCodeRef { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (!(await fusionCache.TryGetAsync<string>($"Key2FA:{Key2FA}")).HasValue)
        {
            navigationManager.NavigateTo("/Login", forceLoad: true);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) { await SecurityCodeRef.FocusAsync(); }
    }

    private sealed class InputModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        public string TwoFactorCode { get; set; } = string.Empty;

        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }
    }
}
