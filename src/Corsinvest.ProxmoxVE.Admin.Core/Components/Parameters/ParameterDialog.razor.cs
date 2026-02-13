/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Parameters;

public partial class ParameterDialog(DialogService dialogService)
{
    [Parameter] public string Icon { get; set; } = default!;
    [Parameter] public string Title { get; set; } = default!;
    [Parameter] public string Description { get; set; } = default!;
    [Parameter] public IEnumerable<ParameterMetadata> Parameters { get; set; } = default!;
    [Parameter] public Dictionary<string, object?> Values { get; set; } = [];
    [Parameter] public EventCallback<Dictionary<string, object?>> ValuesChanged { get; set; } = default!;
    [Parameter] public Func<ParameterMetadata, Task<DataSourceContext>>? GetDataSourceContext { get; set; }

    private Dictionary<string, ParameterEditor> Refs { get; set; } = [];
    private Dictionary<string, bool> IsValids { get; set; } = [];

    protected override void OnInitialized()
    {
        foreach (var item in Parameters)
        {
            if (!IsValids.ContainsKey(item.Id)) { IsValids[item.Id] = false; }
            if (!Refs.ContainsKey(item.Id)) { Refs[item.Id] = null!; }
            if (!Values.ContainsKey(item.Id)) { Values[item.Id] = null!; }
        }
    }

    private bool IsValid() => IsValids.Values.All(a => a);

    private async Task ValueChagedAsync(ParameterMetadata param)
    {
        var dependentParams = Parameters
            .Where(p => p.Type == ParameterType.Select && p.Options?.DataSource != null && p.Id != param.Id);

        foreach (var item in dependentParams)
        {
            await Refs[item.Id].InvlaidateCacheAsync();
        }

        if (ValuesChanged.HasDelegate) { await ValuesChanged.InvokeAsync(Values); }

        StateHasChanged();
    }

    private void OnExecute() => dialogService.Close(Values);
}
