/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.DependencyInjection;
using FluentResults;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IPveUtilityService : IScopedDependency
{
    Task<IEnumerable<Result>> FreeMemory(string clusterName, IEnumerable<string> nodes);
    Task<Result> BlinkDiskLed(string clusterName, string node, string devPath, bool blink);
}