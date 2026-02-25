/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.AppTokens;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth.Permissions;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class AppTokenPermissionsGrid(IAppTokenService appTokenService,
                                             DialogService dialogService,
                                             IPermissionsSummaryDialogService permissionsSummaryDialogService)
{
    [Parameter] public IEnumerable<Permission> Permissions { get; set; } = [];

    private IEnumerable<TokenRow> Items { get; set; } = [];
    private IList<TokenRow> SelectedItems { get; set; } = [];

    private class TokenRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsExpired { get; set; }
    }

    protected override async Task OnParametersSetAsync() => await RefreshAsync();

    public async Task RefreshAsync()
    {
        var tokens = await appTokenService.GetByPermissionKeysAsync([.. Permissions.Select(p => p.Key)]);
        Items = tokens.Select(a => new TokenRow
        {
            Id = a.Id,
            Name = a.Name,
            IsActive = a.IsActive,
            CreatedAt = a.CreatedAt,
            ExpiresAt = a.ExpiresAt,
            IsExpired = a.IsExpired
        });

        StateHasChanged();
    }

    private static void RowRender(RowRenderEventArgs<TokenRow> args)
    {
        if (!args.Data!.IsActive || args.Data.IsExpired) { args.SetRowStyleError(); }
    }

    private async Task RegenerateAsync()
    {
        if (!SelectedItems.Any()) { return; }
        if (!await dialogService.ConfirmAsync(L["Regenerate token? The current token will be invalidated."], L["Regenerate"], true)) { return; }

        var rawToken = await appTokenService.RegenerateAsync(SelectedItems[0].Id);
        await dialogService.OpenCopyValueAsync(L["Token regenerated — save it now!"],
                                               rawToken,
                                               L["Token"],
                                               L["This token will not be shown again. Copy it now:"]);
    }
}
