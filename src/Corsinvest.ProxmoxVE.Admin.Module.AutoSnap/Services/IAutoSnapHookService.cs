/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.AutoSnap.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Services;

public interface IAutoSnapHookService
{
    Task ExecuteAsync(JobSchedule job, PhaseEventArgs phaseEvent, TextWriter log);
}
