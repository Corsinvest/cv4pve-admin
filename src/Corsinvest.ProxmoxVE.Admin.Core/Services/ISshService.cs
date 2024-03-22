/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.DependencyInjection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface ISshService : IScopedDependency
{
    IEnumerable<(int ExitCode, string StdOut, string Error)> Execute(string host,
                                                                          int port,
                                                                          string username,
                                                                          string password,
                                                                          IEnumerable<string> commands);

    IEnumerable<(int ExitCode, string StdOut, string Error)> Execute(ClusterOptions clusterOptions,
                                                                     string host,
                                                                     int port,
                                                                     IEnumerable<string> commands);
}