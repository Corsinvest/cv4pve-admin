/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using CronExpressionDescriptor;
using Cronos;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class CronHelper
{
    public static DateTimeOffset? NextOccurrence(string cronExpression)
        => IsValid(cronExpression)
                ? CronExpression.Parse(cronExpression).GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local)!.Value
                : null;

    public static bool IsValid(string cronExpression)
    {
        try
        {
            CronExpression.Parse(cronExpression);
            return true;
        }
        catch { }
        return false;
    }

    public static string GetDescription(string cronExpression)
    {
        var ret = string.Empty;
        if (IsValid(cronExpression))
        {
            try
            {
                ret = ExpressionDescriptor.GetDescription(cronExpression);
            }
            catch
            {
                ret = ExpressionDescriptor.GetDescription(CronExpression.Parse(cronExpression).ToString());
            }
        }
        return ret;
    }
}
