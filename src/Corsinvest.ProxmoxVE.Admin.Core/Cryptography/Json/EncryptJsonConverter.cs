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
            logger?.LogWarning(ex, "Failed to decrypt a protected field — data may have been encrypted with a different key. The field will be reset to null. Re-save the settings to re-encrypt with the current key.");
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value == null
                                    ? null
                                    : protector.Protect(value));
}
