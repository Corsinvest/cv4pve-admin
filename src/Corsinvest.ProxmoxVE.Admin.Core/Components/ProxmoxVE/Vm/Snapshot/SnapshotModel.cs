namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm.Snapshot;

public class SnapshotModel
{
    public bool HasVmStatus { get; set; }

    [Required]
    [RegularExpression(pattern: "^[A-Za-z][A-Za-z0-9_]{1,}$",
                       ErrorMessage = " 'A-Z', 'a-z', '0-9', '_', Minimum characters: 2, Must start with: letter")]
    public string Name { get; set; } = default!;

    public string Description { get; set; } = default!;
    public bool VmStatus { get; set; }
}
