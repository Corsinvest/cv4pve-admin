/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class IdentityExtensions
{
    public static async Task<ApplicationRole> CreateAsync(this RoleManager<ApplicationRole> roleManager,
                                                          Role role,
                                                          IPermissionService permissionService)
    {
        var item = await roleManager.CreateAsync(role.Key, role.Description, role.BuiltIn, role.Default);
        await roleManager.AddPermissionsAsync(item, role.Permissions, permissionService);
        return item;
    }

    public static async Task<SignInResult> TwoFactorAuthenticatorSignInExAsync(this SignInManager<ApplicationUser> signInManager,
                                                                               string userName,
                                                                               string authenticatorCode,
                                                                               bool rememberMe,
                                                                               bool rememberMachine)
    {
        var user = await signInManager.UserManager.FindByNameAsync(userName);
        return user == null
            ? SignInResult.Failed
            : !user.IsActive
                ? SignInResult.NotAllowed
                : await signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, rememberMachine);
    }

    public static async Task<SignInResult> PasswordSignInExAsync(this SignInManager<ApplicationUser> signInManager,
                                                                 string userName,
                                                                 string password,
                                                                 bool isPersistent,
                                                                 bool lockoutOnFailure)
    {
        var user = await signInManager.UserManager.FindByNameAsync(userName);
        return user == null
            ? SignInResult.Failed
            : !user.IsActive
                ? SignInResult.NotAllowed
                : await signInManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
    }

    public static async Task<ApplicationRole> CreateAsync(this RoleManager<ApplicationRole> roleManager,
                                                          string name,
                                                          string description,
                                                          bool builtIn,
                                                          bool @default)
    {
        var role = await roleManager.FindByNameAsync(name);
        if (role == null)
        {
            //create role
            role = new ApplicationRole
            {
                Name = name,
                Description = description,
                BuiltIn = builtIn,
                Default = @default
            };

            await roleManager.CreateAsync(role);
        }

        return role;
    }

    public static async Task AddPermissionsAsync(this RoleManager<ApplicationRole> roleManager,
                                                 ApplicationRole role,
                                                 IEnumerable<Permission> permissions,
                                                 IPermissionService permissionService)
    {
        if (permissions.Any())
        {
            //create permissions
            var permissionsIndDb = (await roleManager.GetClaimsAsync(role))
                                        .Where(a => a.Type == ApplicationClaimTypes.Permission)
                                        .Select(a => a.Value)
                                        .ToList();

            foreach (var item in permissions.Where(a => !permissionsIndDb.Contains(a.Key)))
            {
                await roleManager.AddClaimAsync(role, new Claim(ApplicationClaimTypes.Permission, item.Key));
            }

            await permissionService.AddForRoleAsync(role.Id, permissions.Select(a => new PermissionData(ApplicationHelper.AllClusterName,
                                                                                                        a.Key,
                                                                                                        "*",
                                                                                                        true,
                                                                                                        role.BuiltIn)));
        }
    }

    //public static async Task RemovePermissionsAsync(this RoleManager<ApplicationRole> roleManager, ApplicationRole role, IEnumerable<string> permissions)
    //{
    //    //create permissions
    //    var permissionsIndDb = (await roleManager.GetClaimsAsync(role))
    //                                .Where(a => a.Type == ApplicationClaimTypes.Permission)
    //                                .ToArray();

    //    foreach (var item in permissionsIndDb.Where(a => permissions.Contains(a.Value)))
    //    {
    //        await roleManager.RemoveClaimAsync(role, item);
    //    }
    //}

    public static async Task<IdentityResult> AddToRolesExAsync(this UserManager<ApplicationUser> userManager,
                                                               ApplicationUser user,
                                                               IEnumerable<string> roles,
                                                               IPermissionService permissionService)
    {
        var rolesIndDb = await userManager.GetRolesAsync(user);
        var toAdd = roles.Where(a => !rolesIndDb.Contains(a)).ToList();
        if (toAdd.Count == 0) { return IdentityResult.Success; }

        var result = await userManager.AddToRolesAsync(user, toAdd);
        if (result.Succeeded) { await permissionService.InvalidateUserAsync(user.Id); }
        return result;
    }

    public static async Task<IdentityResult> SyncRolesAsync(this UserManager<ApplicationUser> userManager,
                                                             ApplicationUser user,
                                                             IEnumerable<string> roles,
                                                             IPermissionService permissionService)
    {
        var rolesList = roles.ToList();
        var rolesInDb = await userManager.GetRolesAsync(user);
        var toRemove = rolesInDb.Except(rolesList).ToList();
        var toAdd = rolesList.Except(rolesInDb).ToList();

        var mutated = false;

        if (toRemove.Count > 0)
        {
            var result = await userManager.RemoveFromRolesAsync(user, toRemove);
            if (!result.Succeeded) { return result; }
            mutated = true;
        }

        if (toAdd.Count > 0)
        {
            var result = await userManager.AddToRolesAsync(user, toAdd);
            if (!result.Succeeded) { return result; }
            mutated = true;
        }

        if (mutated) { await permissionService.InvalidateUserAsync(user.Id); }
        return IdentityResult.Success;
    }

    public static async Task<IdentityResult> DeleteExAsync(this UserManager<ApplicationUser> userManager,
                                                           ApplicationUser user,
                                                           IPermissionService permissionService)
    {
        // Delete all user permissions
        var permissions = await permissionService.GetUserPermissionsAsync(user.Id);
        if (permissions.Count != 0)
        {
            await permissionService.RemoveForUserAsync(user.Id,
                permissions.Select(a => new PermissionData(a.ClusterName, a.PermissionKey, a.Path)));
        }

        // Delete user
        var result = await userManager.DeleteAsync(user);

        // Invalidate cache unconditionally (even if user had no direct permissions,
        // cached UserRoles/UserId keys may still exist from previous reads)
        if (result.Succeeded) { await permissionService.InvalidateUserAsync(user.Id); }
        return result;
    }

    public static async Task<IdentityResult> DeleteExAsync(this RoleManager<ApplicationRole> roleManager,
                                                            ApplicationRole role,
                                                            IPermissionService permissionService)
    {
        // Delete all role permissions
        var permissions = await permissionService.GetRolePermissionsAsync(role.Id);
        if (permissions.Count != 0)
        {
            await permissionService.RemoveForRoleAsync(role.Id,
                permissions.Select(a => new PermissionData(a.ClusterName, a.PermissionKey, a.Path)));
        }

        // Delete role
        var result = await roleManager.DeleteAsync(role);

        // Invalidate cache unconditionally to clear RolesClaims/RolePermissions
        // and UserRoles of all users that had this role
        if (result.Succeeded) { await permissionService.InvalidateRoleAsync(role.Id); }
        return result;
    }

    //public static async Task<IdentityResult> RemoveToRolesExAsync(this UserManager<ApplicationUser> userManager,
    //                                                              ApplicationUser user,
    //                                                              IEnumerable<string> roles)
    //{
    //    var rolesIndDb = await userManager.GetRolesAsync(user);
    //    return await userManager.RemoveFromRolesAsync(user, roles.Where(a => rolesIndDb.Contains(a)));
    //}

    //public static async Task<IdentityResult> ResetPasswordAsync(this UserManager<ApplicationUser> userManager, ApplicationUser user, string password)
    //    => await userManager.ResetPasswordAsync(user, await userManager.GeneratePasswordResetTokenAsync(user), password);

    //public static async Task<IdentityResult> RemoveRolesToUserAsync(this UserManager<ApplicationUser> userManager, ApplicationUser user, IEnumerable<string> roles)
    //{
    //    var rolesIndDb = await userManager.GetRolesAsync(user);
    //    return await userManager.RemoveFromRolesAsync(user, roles.Where(a => rolesIndDb.Contains(a)));
    //}

    public static async Task<string> GeneratePasswordResetLinkAsync(this UserManager<ApplicationUser> userManager,
                                                                    ApplicationUser user,
                                                                    NavigationManager navigationManager)
    {
        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        return navigationManager.GetUriWithQueryParameters(navigationManager.ToAbsoluteUri("/ResetPassword").AbsoluteUri,
                                                           new Dictionary<string, object?> { ["code"] = code });
    }

    public static async Task SendPasswordResetAsync(this UserManager<ApplicationUser> userManager,
                                                    ApplicationUser user,
                                                    NavigationManager navigationManager,
                                                    IEmailSender<ApplicationUser> emailSender)
    {
        var link = await userManager.GeneratePasswordResetLinkAsync(user, navigationManager);
        await emailSender.SendPasswordResetLinkAsync(user, user.Email!, link);
    }

    public static string GenerateRandomPassword()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) + "Aa1!";

    public static async Task SendConfirmationAsync(this UserManager<ApplicationUser> userManager,
                                                   ApplicationUser user,
                                                   NavigationManager navigationManager,
                                                   IEmailSender<ApplicationUser> emailSender)
    {
        var userId = await userManager.GetUserIdAsync(user);
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var link = navigationManager.GetUriWithQueryParameters(
            navigationManager.ToAbsoluteUri("/ConfirmEmail").AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });
        await emailSender.SendConfirmationLinkAsync(user, user.Email!, link);
    }
}
