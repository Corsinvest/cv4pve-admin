/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;

public enum TaskItemStatus
{
    Running = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3,
    Abandoned = 4
}
