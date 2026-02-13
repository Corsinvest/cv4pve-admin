/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class ItemStatus
{
    public string Status { get; set; } = default!;
    public int Count { get; set; }
    public string Icon { get; set; } = default!;
    public string? Color { get; set; }
}

public class ItemStatus<TData> : ItemStatus
{
    public TData Data { get; set; } = default!;
}
