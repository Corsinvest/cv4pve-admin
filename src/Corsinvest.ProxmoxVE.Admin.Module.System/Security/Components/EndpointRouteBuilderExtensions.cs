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

    public static string GetUserProfileUrl(string email) => $"/profile-image/{email}";

    private static string BuildErrorUrl(string returnUrl, string errorMessage)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["returnUrl"] = returnUrl;
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

    // These endpoints are required by the Identity Razor components defined in the /Components/Pages directory of this project.
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup(string.Empty);

        accountGroup.MapGet("/profile-image/{email}", (string email) =>
        {
            var filePath = ApplicationUser.GetUserProfileImagePath(email);
            if (File.Exists(filePath))
            {
                var stream = File.OpenRead(filePath);
                return Results.File(stream, MediaTypeNames.Image.Jpeg);
            }
            else
            {
                return Results.NotFound();
            }
        });

        static string FixReturnUrl(string value)
        {
            var returnUrl = value + string.Empty;
            if (returnUrl.StartsWith("NotFound", StringComparison.InvariantCultureIgnoreCase)
                || returnUrl.StartsWith("/NotFound", StringComparison.InvariantCultureIgnoreCase))
            {
                returnUrl = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(returnUrl)) { returnUrl = "/"; }
            return returnUrl;
        }

        accountGroup.MapPost("/Login2fa", async ([FromServices] SignInManager<ApplicationUser> signInManager,
                                                        [FromServices] IAuditService auditService,
                                                        IFusionCache fusionCache,
                                                        [FromForm] InputLogin2fa model) =>
        {
            var url = string.Empty;
            var tmpReturnUrl = FixReturnUrl(model.ReturnUrl!);

            var key = $"Key2FA:{model.Key2FA}";
            var userName = await fusionCache.TryGetAsync<string>(key);
            if (userName.HasValue)
            {
                await fusionCache.RemoveAsync(key);
            }
            else
            {
                return TypedResults.Redirect(BuildErrorUrl(tmpReturnUrl, "Invalid data"));
            }

            var authenticatorCode = model.TwoFactorCode!.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await signInManager.TwoFactorAuthenticatorSignInExAsync(userName.Value, authenticatorCode, model.RememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                await auditService.LogAsync(userName.Value, "Login-TwoFactor", true);
                url = tmpReturnUrl;
            }
            else if (result.IsLockedOut)
            {
                await auditService.LogAsync(userName.Value, "Login-TwoFactor", false, "Account locked out");
                url = BuildErrorUrl(tmpReturnUrl, "This account has been locked out, please try again later");
            }
            else if (result.IsNotAllowed)
            {
                await auditService.LogAsync(userName.Value, "Login-TwoFactor", false, "Account not allowed");
                url = BuildErrorUrl(tmpReturnUrl, "This account is not allowed, please try again later");
            }
            else
            {
                await auditService.LogAsync(userName.Value, "Login-TwoFactor", false, "Invalid authenticator code");
                url = BuildErrorUrl(tmpReturnUrl, "Invalid authenticator code entered");
            }

            return TypedResults.Redirect(url);
        });

        accountGroup.MapPost("/Login", async ([FromServices] SignInManager<ApplicationUser> signInManager,
                                                     [FromServices] IAuditService auditService,
                                                     IFusionCache fusionCache,
                                                     [FromForm] InputLogin model,
                                                     [FromQuery] string? returnUrl) =>
        {
            var url = string.Empty;
            var tmpReturnUrl = FixReturnUrl(returnUrl!);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await signInManager.PasswordSignInExAsync(model.UserName, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                await auditService.LogAsync(model.UserName, "Login-Password", true);
                url = tmpReturnUrl;
            }
            else if (result.RequiresTwoFactor)
            {
                var key = $"{Guid.NewGuid():N}";
                await fusionCache.SetAsync($"Key2FA:{key}", model.UserName, TimeSpan.FromMinutes(3));

                var query = HttpUtility.ParseQueryString(string.Empty);
                query["Key2FA"] = key;
                query["returnUrl"] = tmpReturnUrl;
                query["rememberMe"] = model.RememberMe.ToString();
                url = $"/LoginWith2fa?{query}";
            }
            else if (result.IsLockedOut)
            {
                await auditService.LogAsync(model.UserName, "Login-Password", false, "Account locked out");
                url = BuildErrorUrl(tmpReturnUrl, "This account has been locked out, please try again later");
            }
            else if (result.IsNotAllowed)
            {
                await auditService.LogAsync(model.UserName, "Login-Password", false, "Account not allowed");
                url = BuildErrorUrl(tmpReturnUrl, "This account is not allowed, please try again later");
            }
            else
            {
                await auditService.LogAsync(model.UserName, "Login-Password", false, "Invalid credentials");
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

            if (!string.IsNullOrEmpty(userName)) { await auditService.LogAsync(userName, "Logout", true); }

            return TypedResults.LocalRedirect($"~/{returnUrl}");
        });

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
