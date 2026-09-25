/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Cryptography.Json;

public class EncryptJsonConverter(IDataProtector protector, ILogger<EncryptJsonConverter>? logger = null) : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var encryptedValue = reader.GetString();
        if (encryptedValue == null) { return null; }

        try
        {
            return protector.Unprotect(encryptedValue);
        }
        catch (Exception ex)
        {
            // Every Data Protection payload starts with the magic header 0x09F0C9F0, "CfDJ8" in base64url.
            // A value without it was saved before the field became encrypted: it is returned as-is and
            // gets encrypted on the next save, so marking an existing field [Encrypt] loses nothing.
            if (!encryptedValue.StartsWith("CfDJ8", StringComparison.Ordinal)) { return encryptedValue; }

            logger?.LogWarning(ex, "Failed to decrypt a protected field — data may have been encrypted with a different key. The field will be reset to null. Re-save the settings to re-encrypt with the current key.");
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value == null
                                    ? null
                                    : protector.Protect(value));
}
