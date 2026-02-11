namespace Corsinvest.ProxmoxVE.Admin.Core.Notifier;

public interface INotifierConfigurationsSettings
{
    [Display(Name = "Notifiers")]
    IEnumerable<string> NotifierConfigurations { get; set; }
}
