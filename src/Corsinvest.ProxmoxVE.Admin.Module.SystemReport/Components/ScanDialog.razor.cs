namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Components;

public partial class ScanDialog : IModelParameter<JobResult>
{
    [Parameter] public JobResult Model { get; set; } = default!;

    private List<VmFeature> VmFeatures
    {
        get => Model.VmFeatures == VmFeature.All
                    ? []
                    : [.. Enum.GetValues(typeof(VmFeature))
                              .Cast<VmFeature>()
                              .Where(f => f != VmFeature.All && Model.VmFeatures.HasFlag(f))];
        set
        {
            Model.VmFeatures = value == null || value.Count == 0
                                    ? VmFeature.All
                                    : value.Aggregate((acc, f) => acc | f);
        }
    }

    private List<NodeFeature> NodeFeatures
    {
        get => Model.NodeFeatures == NodeFeature.All
                    ? []
                    : [.. Enum.GetValues(typeof(NodeFeature))
                              .Cast<NodeFeature>()
                              .Where(f => f != NodeFeature.All && Model.NodeFeatures.HasFlag(f))];

        set => Model.NodeFeatures = value == null || value.Count == 0
                                        ? NodeFeature.All
                                        : value.Aggregate((acc, f) => acc | f);
    }

    private List<StorageFeature> StorageFeatures
    {
        get => Model.StorageFeatures == StorageFeature.All
                    ? []
                    : [.. Enum.GetValues(typeof(StorageFeature))
                              .Cast<StorageFeature>()
                              .Where(f => f != StorageFeature.All && Model.StorageFeatures.HasFlag(f))];

        set => Model.StorageFeatures = value == null || value.Count == 0
                                          ? StorageFeature.All
                                          : value.Aggregate((acc, f) => acc | f);
    }
}
