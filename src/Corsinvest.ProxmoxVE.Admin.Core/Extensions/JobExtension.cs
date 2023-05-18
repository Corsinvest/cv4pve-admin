/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.BackgroundJob;
using System.Linq.Expressions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class JobExtension
{
    public static void ScheduleOrRemove<T>(this IJobService JobService,
                                           Expression<Func<T, Task>> methodCall,
                                           string cronExpression,
                                           bool enabled,
                                           string clusterName,
                                           params object?[] args)
    {
        var jobId = JobHelper.GetJobId<T>(clusterName, args);
        if (enabled)
        {
            JobService.Schedule<T>(jobId, methodCall, cronExpression,TimeZoneInfo.Local);
        }
        else
        {
            JobService.RemoveIfExists(jobId);
        }
    }
}