/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class SshService : ISshService
{
    public IEnumerable<(int ExitCode, string StdOut, string Error)> Execute(string host,
                                                                            int port,
                                                                            string username,
                                                                            string password,
                                                                            IEnumerable<string> commands)
        => SshHelper.Execute(host, port, username, password, commands);

    public IEnumerable<(int ExitCode, string StdOut, string Error)> Execute(ClusterOptions clusterOptions,
                                                                            string host,
                                                                            int port,
                                                                            IEnumerable<string> commands)
        => SshHelper.Execute(clusterOptions, host, port, commands);
}