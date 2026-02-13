/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IToolBarUtility<T>
{
    Task ExecuteAsync(string clusterName, T item);
    string Icon { get; }
    bool IsVIsible(T item);
    bool RequireConfirm { get; }
    string Text { get; }
    ToolBarUtilityType Type { get; }
}
