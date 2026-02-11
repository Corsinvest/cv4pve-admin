namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Components;

public partial class JobDialog(IAdminService adminService) : IModelParameter<JobSchedule>
{
    [Parameter] public JobSchedule Model { get; set; } = default!;

    private ICollection<string> VmIds { get; set; } = [];
    private IEnumerable<string> _vmIdsBase = [];

    protected override async Task OnInitializedAsync()
    {
        var client = await adminService[Model.ClusterName].GetPveClientAsync();

        _vmIdsBase = [.. await client.GetVmIdsAsync(true,
                                                    true,
                                                    true,
                                                    true,
                                                    true,
                                                    true)];

        MakeVmIds();
    }

    private void MakeVmIds()
        => VmIds = [.. new List<string>([.. _vmIdsBase, .. Model.VmIdsList])
                        .Distinct()
                        .Order()];

    private void SearchTextChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value))
        {
            var data = VmIds.Where(a => a.Contains(value));
            if (!data.Any())
            {
                MakeVmIds();
                VmIds.Add(value);
                VmIds = [.. VmIds.Distinct().Order()];
            }
        }
    }
}
