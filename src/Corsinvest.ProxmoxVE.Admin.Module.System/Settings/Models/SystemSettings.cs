namespace Corsinvest.ProxmoxVE.Admin.Module.System.Settings.Models;

public class SystemSettings : IId
{
    public int Id { get; set; }
    [Required] public string Context { get; set; } = default!;
    [Required] public string Section { get; set; } = default!;
    [Required] public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
}
