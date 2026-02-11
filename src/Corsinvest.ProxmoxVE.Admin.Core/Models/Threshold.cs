namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class Threshold<T>
{
    /// <summary>
    /// Warning
    /// </summary>
    [DisplayName("Warning")]
    public T Warning { get; set; } = default!;

    /// <summary>
    /// Critical
    /// </summary>
    /// <value></value>
    [DisplayName("Critical")]
    public T Critical { get; set; } = default!;
}
