namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

public sealed class AppSettings
{
    public string AppUrl { get; set; } = "https://corsinvest.it/cv4pve-admin";
    public string AppName { get; set; } = "cv4pve-admin";

    public string LoginTitle { get; set; } = "Welcome";
    public string LoginDescription { get; set; } = "Take control of your Proxmox VE";

    public SmtpEmailConfig SmtpEmailConfig { get; set; } = new()
    {
        Host = "smtp.example.com",
        FromAddress = "noreplay@example.com",
        FromDisplayName = "cv4pve-Admin",
    };

    public ExtendedData ExtendedData { get; set; } = [];

    //public bool UpdateUsePrerelease { get; set; }mailkit

    //public string SystemNotifier { get; set; } = default!;

    //[Required]
    //public string Company { get; set; } = default!;
    //public string Email { get; set; } = default!;
    //public string FullName { get; set; } = default!;

    //public List<string> NavMenu { get; set; } = [];
}
