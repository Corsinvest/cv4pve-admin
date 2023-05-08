/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Security.Auth.Permissions;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Storage;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Storage;

public partial class Storages
{
    [Parameter] public string Height { get; set; } = default!;
    [EditorRequired][Parameter] public Func<Task<IEnumerable<StorageItem>>> GetItems { get; set; } = default!;
    [Parameter] public PermissionsRead Permissions { get; set; } = default!;

    [Inject] private IDataGridManager<StorageItem> DataGridManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        DataGridManager.Title = L["Storages"];
        DataGridManager.DefaultSort = new() { [nameof(StorageItem.Storage)] = false };
        DataGridManager.QueryAsync = GetItems;
    }
}