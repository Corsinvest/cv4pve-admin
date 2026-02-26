/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public record SshCommandResult(string Command, int ExitCode, string StdOut, string StdErr, bool IsSshNotConfigured = false)
{
    public bool IsSuccess => ExitCode == 0;
}
