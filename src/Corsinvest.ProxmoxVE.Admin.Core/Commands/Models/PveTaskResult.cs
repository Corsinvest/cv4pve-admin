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
        => (PveTaskResult)CommandResult<Result>.Failure(errorMessage) with { ClusterName = clusterName };

    public static new PveTaskResult Forbidden(string clusterName)
        => (PveTaskResult)CommandResult<Result>.Forbidden() with { ClusterName = clusterName };

    public static new PveTaskResult Unauthorized(string clusterName)
        => (PveTaskResult)CommandResult<Result>.Unauthorized() with { ClusterName = clusterName };
}
