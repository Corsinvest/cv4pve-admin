/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net.Mime;
using System.Web;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using ZiggyCreatures.Caching.Fusion;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Components;

public static class EndpointRouteBuilderExtensions
{
    //public const string LinkLoginCallbackAction = "LinkLoginCallback";
    public const string LoginCallbackAction = "LoginCallback";
    private const string MessageLockedOut = "This account has been locked out, please try again later";
    private const string MessageNotAllowed = "This account is not allowed to sign in, contact your administrator";

    public static string GetUserProfileUrl(string email) => $"/profile-image/{email}";

    /// <summary>
    /// Accept only a local relative path (e.g. "/foo/bar"). Reject null/empty, absolute URLs,
    /// protocol-relative (//host), or any form that could cause open redirect.
    /// Returns a safe default "/" otherwise.
    /// </summary>
    private static string SanitizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) { return "/"; }

        var trimmed = returnUrl.Trim();

        // Must be a relative path starting with single '/' and not '//' (protocol-relative)
        if (!trimmed.StartsWith('/') || trimmed.StartsWith("//")) { return "/"; }

        // Reject scheme-relative like "/\evil.com" or backslash tricks
        if (trimmed.Contains('\\')) { return "/"; }

        return trimmed;
    }

    private static string BuildErrorUrl(string? returnUrl, string errorMessage)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["returnUrl"] = SanitizeReturnUrl(returnUrl);
        query["error"] = errorMessage;
        return $"/Login?{query}";
    }

    private class InputLogin
    {
        [Required] public string UserName { get; set; } = default!;
        [Required] public string Password { get; set; } = default!;
        public bool RememberMe { get; set; } = default!;
    }

    private class InputLogin2fa
    {
        [Required] public string Key2FA { get; set; } = default!;
        [Required] public string? TwoFactorCode { get; set; }

        public bool RememberMachine { get; set; }
        public bool RememberMe { get; set; } = default!;
        public string ReturnUrl { get; set; } = default!;
    }

    private static string BuildTwoFactorUrl(string key2FA, string returnUrl, bool rememberMe, bool recovery = false, string? errorMessage = null)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["Key2FA"] = key2FA;
        query["returnUrl"] = returnUrl;
        query["rememberMe"] = rememberMe.ToString();
        if (recovery) { query["recovery"] = bool.TrueString; }
        if (errorMessage != null) { query["error"] = errorMessage; }
        return $"/LoginWith2fa?{query}";
    }

    private static void AppendCultureCookie(HttpContext httpContext, ISettingsService settingsService)
        => httpContext.Response.AppendCultureCookie(settingsService.Get<UserSettings>(forCurrentUser: true).Culture);

    // These endpoints are required by the Identity Razor components defined in the /Components/Pages directory of this project.
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup(string.Empty);

        accountGroup.MapGet("/profile-image/{email}", (string email) =>
        {
            var filePath = Path.GetFullPath(ApplicationUser.GetUserProfileImagePath(email));
            var baseDir = Path.GetFullPath(ApplicationHelper.UserProfileImagesPath);

            if (!filePath.StartsWith(baseDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound();
            }

            if (File.Exists(filePath))
            {
                var stream = File.OpenRead(filePath);
                return Results.File(stream, MediaTypeNames.Image.Jpeg);
            }

            return Results.NotFound();
        }).RequireAuthorization();

        static string FixReturnUrl(string value, HttpRequest request)
        {
            var returnUrl = value + string.Empty;

            // RedirectToLogin sends the absolute address of the page: keep its local part,
            // and only when it points to this host
            if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                returnUrl = string.Equals(uri.Authority, request.Host.Value, StringComparison.OrdinalIgnoreCase)
                                ? uri.PathAndQuery + uri.Fragment
                                : string.Empty;
            }

            if (returnUrl.StartsWith("NotFound", StringComparison.InvariantCultureIgnoreCase)
                || returnUrl.StartsWith("/NotFound", StringComparison.InvariantCultureIgnoreCase))
            {
                returnUrl = string.Empty;
            }

            return SanitizeReturnUrl(returnUrl);
        }

        accountGroup.MapPost("/Login2fa", async ([FromServices] SignInManager<ApplicationUser> signInManager,
                                                        [FromServices] IAuditService auditService,
                                                        [FromServices] ISettingsService settingsService,
                                                        IFusionCache fusionCache,
                                                        HttpContext httpContext,
                                                        [FromForm] InputLogin2fa model) =>
        {
            var tmpReturnUrl = FixReturnUrl(model.ReturnUrl!, httpContext.Request);

            var key = $"Key2FA:{model.Key2FA}";
            var userName = await fusionCache.TryGetAsync<string>(key);
            if (!userName.HasValue) { return TypedResults.Redirect(BuildErrorUrl(tmpReturnUrl, "Invalid data")); }

            // An authenticator code is 6 digits, a recovery code is longer and keeps its dash
            var code = (model.TwoFactorCode + string.Empty).Replace(" ", string.Empty);
            var authenticatorCode = code.Replace("-", string.Empty);
            var isRecoveryCode = authenticatorCode.Length != 6;
            var action = isRecoveryCode ? "Login-RecoveryCode" : "Login-TwoFactor";

            var result = isRecoveryCode
                            ? await signInManager.TwoFactorRecoveryCodeSignInExAsync(userName.Value, code)
                            : await signInManager.TwoFactorAuthenticatorSignInExAsync(userName.Value, authenticatorCode, model.RememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                await fusionCache.RemoveAsync(key);
                await auditService.LogAsync(action, true, $"User '{userName.Value}' logged in");
                AppendCultureCookie(httpContext, settingsService);
                return TypedResults.Redirect(tmpReturnUrl);
            }

            if (result.IsLockedOut || result.IsNotAllowed)
            {
                await fusionCache.RemoveAsync(key);
                await auditService.LogAsync(action, false, $"Account {(result.IsLockedOut ? "locked out" : "not allowed")} for user '{userName.Value}'");
                return TypedResults.Redirect(BuildErrorUrl(tmpReturnUrl, result.IsLockedOut ? MessageLockedOut : MessageNotAllowed));
            }

            // The key stays valid, so a typing mistake does not send the user back to the password
            await auditService.LogAsync(action, false, $"Invalid code for user '{userName.Value}'");
            return TypedResults.Redirect(BuildTwoFactorUrl(model.Key2FA,
                                                           tmpReturnUrl,
                                                           model.RememberMe,
                                                           isRecoveryCode,
                                                           isRecoveryCode ? "Invalid recovery code entered" : "Invalid authenticator code entered"));
        });

        accountGroup.MapPost("/Login", async ([FromServices] SignInManager<ApplicationUser> signInManager,
                                                     [FromServices] IAuditService auditService,
                                                     [FromServices] ISettingsService settingsService,
                                                     IFusionCache fusionCache,
                                                     HttpContext httpContext,
                                                     [FromForm] InputLogin model,
                                                     [FromQuery] string? returnUrl) =>
        {
            var url = string.Empty;
            var tmpReturnUrl = FixReturnUrl(returnUrl!, httpContext.Request);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await signInManager.PasswordSignInExAsync(model.UserName, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                await auditService.LogAsync("Login-Password", true, $"User '{model.UserName}' logged in");

                AppendCultureCookie(httpContext, settingsService);

                url = tmpReturnUrl;
            }
            else if (result.RequiresTwoFactor)
            {
                var key = $"{Guid.NewGuid():N}";
                await fusionCache.SetAsync($"Key2FA:{key}", model.UserName, TimeSpan.FromMinutes(3));

                url = BuildTwoFactorUrl(key, tmpReturnUrl, model.RememberMe);
            }
            else if (result.IsLockedOut)
            {
                await auditService.LogAsync("Login-Password", false, $"Account locked out for user '{model.UserName}'");
                url = BuildErrorUrl(tmpReturnUrl, MessageLockedOut);
            }
            else if (result.IsNotAllowed)
            {
                await auditService.LogAsync("Login-Password", false, $"Account not allowed for user '{model.UserName}'");
                url = BuildErrorUrl(tmpReturnUrl, MessageNotAllowed);
            }
            else
            {
                await auditService.LogAsync("Login-Password", false, $"Invalid credentials for user '{model.UserName}'");
                url = BuildErrorUrl(tmpReturnUrl, "Invalid user or password");
            }

            return TypedResults.Redirect(url);
        });

        accountGroup.MapGet("/Logout", async (HttpContext context,
                                                     [FromServices] SignInManager<ApplicationUser> signInManager,
                                                     [FromServices] IAuditService auditService,
                                                     [FromQuery] string? returnUrl) =>
        {
            var userName = context.User?.Identity?.Name;
            await signInManager.SignOutAsync();

            if (!string.IsNullOrEmpty(userName)) { await auditService.LogAsync("Logout", true, $"User '{userName}' logged out"); }

            return TypedResults.LocalRedirect($"~{SanitizeReturnUrl(returnUrl)}");
        });

        // Changing the password or the 2FA setup changes the security stamp: without a new cookie the session
        // that made the change is closed at the next validation. A cookie is written only from an HTTP response.
        accountGroup.MapGet("/RefreshSignIn", async (HttpContext context,
                                                            [FromServices] SignInManager<ApplicationUser> signInManager,
                                                            [FromQuery] string? returnUrl) =>
        {
            var user = await signInManager.UserManager.GetUserAsync(context.User);
            if (user != null) { await signInManager.RefreshSignInAsync(user); }

            return TypedResults.LocalRedirect($"~{SanitizeReturnUrl(returnUrl)}");
        }).RequireAuthorization();

        accountGroup.MapGet("/ForgetTwoFactorBrowser", async ([FromServices] SignInManager<ApplicationUser> signInManager,
                                                                     [FromServices] IAuditService auditService,
                                                                     [FromQuery] string? returnUrl) =>
        {
            await signInManager.ForgetTwoFactorClientAsync();
            await auditService.LogAsync("TwoFactor.ForgetBrowser", true);

            return TypedResults.LocalRedirect($"~{SanitizeReturnUrl(returnUrl)}");
        }).RequireAuthorization();

        accountGroup.MapPost("/PerformExternalLogin", (HttpContext context,
                                                              [FromServices] SignInManager<ApplicationUser> signInManager,
                                                              [FromForm] string provider,
                                                              [FromForm] string returnUrl) =>
        {
            IEnumerable<KeyValuePair<string, StringValues>> query =
            [
                new("ReturnUrl", returnUrl),
                new("Action", LoginCallbackAction)
            ];

            var redirectUrl = UriHelper.BuildRelative(context.Request.PathBase, "/ExternalLogin", QueryString.Create(query));
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return TypedResults.Challenge(properties, [provider]);
        });

        return accountGroup;
    }
}

//internal static class IdentityComponentsEndpointRouteBuilderExtensions
//{
//    // These endpoints are required by the Identity Razor components defined in the /Components/Pages directory of this project.
//    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
//    {
//        ArgumentNullException.ThrowIfNull(endpoints);

//        var accountGroup = endpoints.MapGroup(string.Empty);

//        accountGroup.MapPost("/PerformExternalLogin", (HttpContext context,
//                                                              [FromServices] SignInManager<ApplicationUser> signInManager,
//                                                              [FromForm] string provider,
//                                                              [FromForm] string returnUrl) =>
//        {
//            IEnumerable<KeyValuePair<string, StringValues>> query =
//            [
//                new("ReturnUrl", returnUrl),
//                new("Action", ExternalLogin.LoginCallbackAction)
//            ];

//            var redirectUrl = UriHelper.BuildRelative(context.Request.PathBase, "/ExternalLogin", QueryString.Create(query));
//            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
//            return TypedResults.Challenge(properties, [provider]);
//        });

//        accountGroup.MapGet("/Logout", async (SignInManager<ApplicationUser> signInManager,
//                                                     [FromQuery] string? returnUrl) =>
//        {
//            await signInManager.SignOutAsync();
//            return TypedResults.LocalRedirect($"~/{returnUrl}");
//        });

//        //accountGroup.MapPost("/Logout", async ([FromServices] SignInManager<ApplicationUser> signInManager,
//        //                                       [FromForm] string returnUrl) =>
//        //{
//        //    await signInManager.SignOutAsync();
//        //    return TypedResults.LocalRedirect($"~/{returnUrl}");
//        //});

//        var manageGroup = accountGroup.MapGroup("/Manage").RequireAuthorization();

//        manageGroup.MapPost("/LinkExternalLogin", async (HttpContext context,
//                                                                [FromServices] SignInManager<ApplicationUser> signInManager,
//                                                                [FromForm] string provider) =>
//        {
//            // Clear the existing external cookie to ensure a clean login process
//            await context.SignOutAsync(IdentityConstants.ExternalScheme);

//            var redirectUrl = UriHelper.BuildRelative(context.Request.PathBase,
//                                                      "/Manage/ExternalLogins",
//                                                      QueryString.Create("Action", ExternalLogins.LinkLoginCallbackAction));

//            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, signInManager.UserManager.GetUserId(context.User));
//            return TypedResults.Challenge(properties, [provider]);
//        });

//        var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
//        var downloadLogger = loggerFactory.CreateLogger("DownloadPersonalData");

//        manageGroup.MapPost("/DownloadPersonalData", async (HttpContext context,
//                                                                   [FromServices] UserManager<ApplicationUser> userManager) =>
//        {
//            var user = await userManager.GetUserAsync(context.User);
//            if (user is null)
//            {
//                return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
//            }

//            downloadLogger.LogInformation("User with ID '{UserId}' asked for their personal data.", await userManager.GetUserIdAsync(user));

//            // Only include personal data for download
//            var personalData = typeof(ApplicationUser).GetProperties()
//                                                      .Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)))
//                                                      .ToDictionary(a => a.Name, a => a.GetValue(user)?.ToString() ?? "null");

//            personalData.AddRange((await userManager.GetLoginsAsync(user))
//                                    .ToDictionary(a => $"{a.LoginProvider} external login provider key", a => a.ProviderKey));

//            personalData.Add("Authenticator Key", (await userManager.GetAuthenticatorKeyAsync(user))!);
//            var fileBytes = JsonSerializer.SerializeToUtf8Bytes(personalData);

//            context.Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");
//            return TypedResults.File(fileBytes, contentType: "application/json", fileDownloadName: "PersonalData.json");
//        });

//        return accountGroup;
//    }
//}
