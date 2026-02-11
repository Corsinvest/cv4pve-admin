namespace Corsinvest.ProxmoxVE.Admin.Module.Profile.Components;

public partial class ChangePassword(UserManager<ApplicationUser> userManager,
                                    ICurrentUserService currentUserService,
                                    NotificationService notificationService)
{
    private string? Error { get; set; }
    private InputModel Model { get; set; } = new();

    private class InputModel
    {
        [Required, DataType(DataType.Password)]
        public string OldPassword { get; set; } = default!;

        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; } = default!;

        [DataType(DataType.Password), Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = default!;
    }

    private async Task SubmitAsync()
    {
        Error = null;

        var user = (await userManager.FindByIdAsync(currentUserService.UserId))!;
        var result = await userManager.ChangePasswordAsync(user, Model.OldPassword, Model.NewPassword);
        if (result.Succeeded)
        {
            await userManager.UpdateSecurityStampAsync(user);
            notificationService.Success(L["Your password has been changed"]);
        }
        else
        {
            Error = result.Errors.Select(a => a.Description).JoinAsString(",");
            notificationService.Error("Error", Error);
            return;
        }
    }
}
