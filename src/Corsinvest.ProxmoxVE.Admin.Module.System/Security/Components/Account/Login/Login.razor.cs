/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components.Account.Login;

public partial class Login(NavigationManager navigationManager,
                           IExternalAuthenticationService externalAuthenticationService)
{
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }
    [SupplyParameterFromQuery] private string? Error { get; set; }
    [SupplyParameterFromQuery(Name = "domain")] private string? Domain { get; set; }

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
    private IReadOnlyList<ExternalLoginButton> ExternalLogins { get; set; } = [];
    private IReadOnlyList<string> Domains { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
#if DEBUG
        Input.Username = ApplicationHelper.DefaultAdminUsername;
        Input.Password = ApplicationHelper.DefaultUserPassword;
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

        var buttons = await externalAuthenticationService.GetLoginButtonsAsync();
        ExternalLogins = [.. buttons.Where(a => !a.IsDomain)];
        Domains = [.. buttons.Where(a => a.IsDomain).Select(a => a.Name)];
        Domain = Domains.FirstOrDefault(a => a.Equals(Domain, StringComparison.OrdinalIgnoreCase));
    }

    private static string Tooltip(ExternalLoginButton item)
        => string.IsNullOrWhiteSpace(item.Description) ? item.Name : $"{item.Name}: {item.Description}";

    private void ForgotPassword() => navigationManager.NavigateTo("/ForgotPassword");
}
