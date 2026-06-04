/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;

public record PveTaskResult : CommandResult<Result>
{
    /// <summary>
    /// The Proxmox VE task UPID (Unique Process ID).
    /// Format: UPID:node:pid:pstart:starttime:type:id:user@realm:
    /// </summary>
    public string? Upid => Data?.Response?.data;

    public string ClusterName { get; init; } = default!;

    public static PveTaskResult Success(Result result, string clusterName)
        => new()
        {
            Status = result.IsSuccessStatusCode ? CommandResultStatus.Success : CommandResultStatus.Failed,
            Data = result,
            ClusterName = clusterName
        };

    public static PveTaskResult Failure(string clusterName, string errorMessage)
        => new() { Status = CommandResultStatus.Failed, ErrorMessage = errorMessage, ClusterName = clusterName };

    public static new PveTaskResult Forbidden(string clusterName)
        => new() { Status = CommandResultStatus.Forbidden, ErrorMessage = "Operation not permitted", ClusterName = clusterName };

    public static new PveTaskResult Unauthorized(string clusterName)
        => new() { Status = CommandResultStatus.Unauthorized, ErrorMessage = "Authentication required", ClusterName = clusterName };
}
