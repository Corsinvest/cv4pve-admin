using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.DataProtection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Cryptography.Json;

public class EncryptAttributeJsonTypeInfoResolver(IServiceProvider serviceProvider) : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
        {
            foreach (var property in jsonTypeInfo.Properties)
            {
                if (property.AttributeProvider?.IsDefined(typeof(EncryptAttribute), true) == true)
                {
                    var dataProtectionProvider = serviceProvider.GetRequiredService<IDataProtectionProvider>();
                    var protector = dataProtectionProvider.CreateProtector("EncryptPropertyConverter");
                    property.CustomConverter = new EncryptJsonConverter(protector);
                }
            }
        }

        return jsonTypeInfo;
    }
}
