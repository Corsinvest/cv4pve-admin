/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Security.Claims;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Microsoft.AspNetCore.Identity;

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

            await permissionService.AddForRoleAsync(role.Id, permissions.Select(a => (ApplicationHelper.AllClusterName, a.Key, "*", false, role.BuiltIn)));
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
                                                               IEnumerable<string> roles)
    {
        var rolesIndDb = await userManager.GetRolesAsync(user);
        return await userManager.AddToRolesAsync(user, roles.Where(a => !rolesIndDb.Contains(a)));
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
                permissions.Select(a => (a.ClusterName, a.PermissionKey, a.Path)));
        }

        // Delete user
        return await userManager.DeleteAsync(user);
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
                permissions.Select(a => (a.ClusterName, a.PermissionKey, a.Path)));
        }

        // Delete role
        return await roleManager.DeleteAsync(role);
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
}
