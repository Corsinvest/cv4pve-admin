/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Storage;

public partial class Summary : IRefreshableData
{
    [EditorRequired, Parameter] public IClusterResourceStorage Storage { get; set; } = default!;
    protected override Task OnInitializedAsync() => RefreshDataAsync();
    public Task RefreshDataAsync() => Task.CompletedTask;
}
