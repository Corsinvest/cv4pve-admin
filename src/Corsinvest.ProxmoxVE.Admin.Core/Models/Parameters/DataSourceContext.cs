/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;

public record DataSourceContext(Dictionary<string, object?> Parameters)
{
    public string? ClusterName
    {
        get => Parameters.TryGetValue(nameof(IClusterName.ClusterName), out var value)
            ? value as string
            : null;

        set => Parameters[nameof(IClusterName.ClusterName)] = value;
    }
}
