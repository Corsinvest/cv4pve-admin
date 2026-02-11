namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IModelParameter<T>
{
    [Parameter] T Model { get; set; }
}
