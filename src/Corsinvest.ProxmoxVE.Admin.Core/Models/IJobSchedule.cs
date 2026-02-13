/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IJobSchedule : IEnabled
{
    string CronExpression { get; set; }
    string CronExpressionDescriptor { get; }
    bool CronExpressionIsValid { get; }
    DateTimeOffset? CronNextExecution { get; }
}
