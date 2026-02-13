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

    public string ClusterName { get; private set; }=default!;

    public static PveTaskResult Success(Result result, string clusterName)
        => new()
        {
            IsSuccess = result.IsSuccessStatusCode,
            Data = result,
            ClusterName = clusterName
        };

    public static PveTaskResult Failure(string clusterName, string errorMessage)
        => new()
        {
            IsSuccess = false,
            ClusterName = clusterName,
            ErrorMessage = errorMessage
        };
}
