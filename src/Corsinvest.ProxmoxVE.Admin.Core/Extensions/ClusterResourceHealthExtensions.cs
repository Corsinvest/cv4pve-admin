/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class ClusterResourceHealthExtensions
{
    extension(IClusterResourceHost resource)
    {
        public double HealthScoreHost
        {
            get
            {
                var cpu = resource.CpuUsagePercentage * 100;
                var ram = resource.MemoryUsagePercentage * 100;
                var disk = resource.DiskSize > 0
                            ? (double)resource.DiskUsage / resource.DiskSize * 100
                            : 0;

                var score = resource.ResourceType switch
                {
                    ClusterResourceType.Node => 100 - ((cpu * 0.4) + (ram * 0.4) + (disk * 0.2)),
                    ClusterResourceType.Vm => resource.Status == PveConstants.StatusVmRunning
                                                ? 100 - ((cpu * 0.5) + (ram * 0.5))
                                                : 0,
                    _ => 100,
                };

                return Math.Round(Math.Clamp(score, 0, 100), 1);
            }
        }
    }

    extension(IClusterResourceStorage resource)
    {
        public double HealthScoreStorage
        {
            get
            {
                var disk = resource.DiskSize > 0
                            ? (double)resource.DiskUsage / resource.DiskSize * 100
                            : 0;

                var score = resource.ResourceType switch
                {
                    ClusterResourceType.Storage => 100 - disk,
                    _ => 100,
                };

                return Math.Round(Math.Clamp(score, 0, 100), 1);
            }
        }
    }

    extension(ClusterResource resource)
    {
        public double HealthScoreCalculated
        {
            get
            {
                var score = resource.ResourceType switch
                {
                    ClusterResourceType.Node or ClusterResourceType.Vm => resource.HealthScoreHost,
                    ClusterResourceType.Storage => resource.HealthScoreStorage,
                    _ => 100,
                };

                return Math.Round(Math.Clamp(score, 0, 100), 1);
            }
        }
    }
}
