using Microsoft.AspNetCore.Components;

namespace Corsinvest.ProxmoxVE.Admin.Module.Notifier;

public abstract class NotifierModuleBase<TSetting, TRender> : ModuleBase
    where TSetting : class
    where TRender : ComponentBase
{
    protected NotifierModuleBase(string serviceName, string? additionalKeywords = null)
    {
        var baseKeywords = $"notification,{serviceName.ToLower()}";
        Keywords = string.IsNullOrEmpty(additionalKeywords)
            ? baseKeywords
            : $"{baseKeywords},{additionalKeywords}";

        ModuleType = ModuleType.Notification;
        Name = serviceName;

        Link = new(this, Name)
        {
            Render = new(typeof(TRender))
        };

        AddAttribute("TypeDef", typeof(TSetting));
    }

    protected override string PermissionBaseKey => $"Notifier.{Name}";
}
