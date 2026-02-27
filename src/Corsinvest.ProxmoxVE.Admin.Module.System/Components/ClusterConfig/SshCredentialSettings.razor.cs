/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Renci.SshNet;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components.ClusterConfig;

public partial class SshCredentialSettings(IJSRuntime jsRuntime, NotificationService notificationService)
{
    [Parameter, EditorRequired] public ClusterSettings Model { get; set; } = default!;

    private string FileInputId { get; } = Guid.NewGuid().ToString();

    private async Task UploadKeyAsync()
        => await jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{FileInputId}').click();");

    private async Task HandleKeyFileSelectedAsync(InputFileChangeEventArgs args)
    {
        var file = args.File;
        if (file == null) { return; }

        await using var stream = file.OpenReadStream(maxAllowedSize: 100 * 1024);
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        if (!IsValidPrivateKey(content, Model.SshCredential.Passphrase))
        {
            notificationService.Error(L["Invalid private key"], L["The file does not contain a valid SSH private key."]);
            return;
        }

        Model.SshCredential.PrivateKeyContent = content;
    }

    private static bool IsValidPrivateKey(string content, string? passphrase)
    {
        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var key = string.IsNullOrEmpty(passphrase)
                        ? new PrivateKeyFile(stream)
                        : new PrivateKeyFile(stream, passphrase);
            return key.Key != null;
        }
        catch
        {
            return false;
        }
    }
}
