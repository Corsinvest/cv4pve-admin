using System.CommandLine;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Microsoft.AspNetCore.Identity;

namespace Corsinvest.ProxmoxVE.Admin.Core.Security;

public static class UserCommands
{
    public static Command CreateUserCommand(IServiceProvider services)
    {
        var userCommand = new Command("user", "User management commands");

        // reset-password subcommand
        var usernameOption = new Option<string>(name: "--username", "-u")
        {
            Description = "Username to reset password for",
            DefaultValueFactory = _ => "admin@local",
            Required = true
        };

        var passwordOption = new Option<string>("--password", "-p")
        {
            Description = "New password",
            Required = true
        };

        var resetPasswordCommand = new Command("reset-password", "Reset user password")
        {
            usernameOption,
            passwordOption
        };

        resetPasswordCommand.SetAction(async action
            => await ResetPasswordAsync(services, action.GetRequiredValue(usernameOption), action.GetRequiredValue(passwordOption)));

        // enable subcommand
        var enableCommand = new Command("enable", "Enable user account")
        {
            usernameOption
        };
        enableCommand.SetAction(async action
            => await SetUserEnabledAsync(services, action.GetRequiredValue(usernameOption), true));

        var disableCommand = new Command("disable", "Disable user account")
        {
            usernameOption
        };
        disableCommand.SetAction(async action
            => await SetUserEnabledAsync(services, action.GetRequiredValue(usernameOption), false));

        userCommand.Add(resetPasswordCommand);
        userCommand.Add(enableCommand);
        userCommand.Add(disableCommand);

        return userCommand;
    }

    private static async Task<int> ResetPasswordAsync(IServiceProvider services, string username, string password)
    {
        try
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                Console.WriteLine($"Error: User '{username}' not found");
                return 1;
            }

            // Reset password
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, password);

            if (result.Succeeded)
            {
                Console.WriteLine($"✓ Password for user '{username}' has been reset successfully");
                return 0;
            }

            Console.WriteLine("Error resetting password:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error.Description}");
            }
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static async Task<int> SetUserEnabledAsync(IServiceProvider services, string username, bool enabled)
    {
        try
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                Console.WriteLine($"Error: User '{username}' not found");
                return 1;
            }

            user.IsActive = enabled;
            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                Console.WriteLine($"✓ User '{username}' has been {(enabled ? "enabled" : "disabled")} successfully");
                return 0;
            }

            Console.WriteLine($"Error {(enabled ? "enabling" : "disabling")} user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error.Description}");
            }
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
