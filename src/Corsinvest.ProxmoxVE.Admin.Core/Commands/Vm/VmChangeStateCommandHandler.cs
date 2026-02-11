using Corsinvest.ProxmoxVE.Admin.Core.Commands.Base;
using Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;

namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Vm;

public record VmChangeStateCommand(string ClusterName, long VmId, VmStatus Action) : ICommand<PveTaskResult>;

public class VmChangeStateCommandHandler(IAdminService adminService, IAuditService auditService)
    : PveCommandHandlerBase<VmChangeStateCommand>(adminService, auditService)
{
    public override async Task<PveTaskResult> HandleAsync(VmChangeStateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var vm = await GetVmAsync(command.ClusterName, command.VmId);
            var result = await VmHelper.ChangeStatusVmAsync(await GetPveClientAsync(command.ClusterName),
                                                            vm.Node,
                                                            vm.VmType,
                                                            vm.VmId,
                                                            command.Action);

            await LogAuditAsync("VmChangeState",
                              true,
                              $"Change status Cluster {command.ClusterName}, VmId {command.VmId}, Action {command.Action}");

            return PveTaskResult.Success(result, command.ClusterName);
        }
        catch (Exception ex)
        {
            return PveTaskResult.Failure(command.ClusterName, $"Failed to {command.Action} VM {command.VmId}: {ex.Message}");
        }
    }
}
