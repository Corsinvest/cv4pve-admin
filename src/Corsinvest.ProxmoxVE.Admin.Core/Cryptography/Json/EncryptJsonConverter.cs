using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Cryptography.Json;

public class EncryptJsonConverter(IDataProtector protector) : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var encryptedValue = reader.GetString();
        return encryptedValue == null
                ? null
                : protector.Unprotect(encryptedValue);
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        => writer.WriteStringValue(value == null
                                    ? null
                                    : protector.Protect(value));
}
