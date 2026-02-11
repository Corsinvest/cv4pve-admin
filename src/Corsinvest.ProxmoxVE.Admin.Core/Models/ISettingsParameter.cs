namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface ISettingsParameter<T>
{
    [Parameter] T Settings { get; set; }
    [Parameter] EventCallback<T> SettingsChanged { get; set; }
}
