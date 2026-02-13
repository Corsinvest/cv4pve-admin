/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Folder.Helpers;

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Models;

public class FolderTaskResult : IClusterName, IId
{
    public int Id { get; set; }
    [Required] public string TaskId { get; set; } = default!;
    [Required] public string ClusterName { get; set; } = default!;
    [Required] public string Node { get; set; } = default!;
    public long Size { get; set; }
    public bool Status { get; set; }
    public string FileName { get; set; } = default!;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public string? Logs { get; set; }

    public TimeSpan Duration
         => End.HasValue
             ? (End - Start).Value
            : TimeSpan.Zero;

    public string GetPath(string clusterName) => FolderHelper.GetPath(clusterName, TaskId, FileName);
    public string GetDirectoryWorkJobId(string clusterName) => FolderHelper.GetDirectoryWork(clusterName, TaskId);
}
