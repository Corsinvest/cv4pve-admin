/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Renci.SshNet;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class SshHelper
{
    public static IEnumerable<(int ExitCode, string StdOut, string Error)> Execute(ClusterOptions clusterOptions,
                                                                                   string host,
                                                                                   int port,
                                                                                   IEnumerable<string> commands)
        => Execute(host,
                   port,
                   clusterOptions.SshCredential.Username!,
                   clusterOptions.SshCredential.Password!,
                   commands);

    public static IEnumerable<(int ExitCode, string StdOut, string Error)> Execute(string host,
                                                                                   int port,
                                                                                   string username,
                                                                                   string password,
                                                                                   IEnumerable<string> commands)
    {
        var ret = new List<(int ExitCode, string StdOut, string Error)>();

        using var sshClient = new SshClient(host, port, username, password);
        sshClient.Connect();
        foreach (var command in commands)
        {
            using var cmd = sshClient.CreateCommand(command);
            ret.Add((cmd.ExitStatus, cmd.Execute(), cmd.Error));
        }
        sshClient.Disconnect();

        return ret;
    }
}