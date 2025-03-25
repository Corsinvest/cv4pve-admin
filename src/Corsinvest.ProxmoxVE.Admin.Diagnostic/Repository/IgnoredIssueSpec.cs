/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Repository;

internal class IgnoredIssueSpec(string clusterName) : ClusterByNameSpec<IgnoredIssue>(clusterName)
{
}
