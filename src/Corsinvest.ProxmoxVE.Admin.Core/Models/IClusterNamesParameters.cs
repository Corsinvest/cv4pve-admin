namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IClusterNamesParameters
{
    [Parameter] IEnumerable<string> ClusterNames { get; set; }
    [Parameter] EventCallback<IEnumerable<string>> ClusterNamesChanged { get; set; }
}
