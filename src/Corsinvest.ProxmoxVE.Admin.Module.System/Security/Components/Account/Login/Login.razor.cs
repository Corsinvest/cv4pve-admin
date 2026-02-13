/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components.Account.Login;

public partial class Login(NavigationManager navigationManager,
                           SignInManager<ApplicationUser> signInManager)
{
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }
    [SupplyParameterFromQuery] private string? Error { get; set; }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Username { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        public bool RememberMe { get; set; }
    }

    private InputModel Input { get; set; } = new();
    private IEnumerable<AuthenticationScheme> ExternalLogins { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
#if DEBUG
        Input.Username = "admin@local";
        Input.Password = "Password123!";
#endif

        if (HttpContext != null && HttpMethods.IsGet(HttpContext.Request.Method))
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                var returnUrl = ReturnUrl;
                if (string.IsNullOrWhiteSpace(returnUrl)) { returnUrl = "/"; }
                navigationManager.NavigateTo(returnUrl);
            }
        }

        ExternalLogins = await signInManager.GetExternalAuthenticationSchemesAsync();
    }

    private void ForgotPassword() => navigationManager.NavigateTo("/ForgotPassword");
}
