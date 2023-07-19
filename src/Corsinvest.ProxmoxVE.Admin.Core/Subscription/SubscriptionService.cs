/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using ApexCharts;
using System.IO;
using System.Xml.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Subscription;

public class SubscriptionService
{
    public async Task<Info> RegisterAsync(string serverId, string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(serverId)) { throw new ArgumentNullException(nameof(serverId)); }
        if (string.IsNullOrWhiteSpace(licenseKey)) { throw new ArgumentNullException(nameof(licenseKey)); }

        var info = new Info() { Status = Status.Invalid };

        try
        {
            using var client = new HttpClient();
            var response = await client.PostAsync("https://shop.corsinvest.it/modules/servers/licensing/verify.php",
                                                  new FormUrlEncodedContent(new Dictionary<string, string>()
                                                  {
                                                      ["licensekey"] = licenseKey,
                                                      ["dir"] = serverId,
                                                      ["domain"] = "www.corsinvest.it",
                                                      ["ip"] = "localhost",
                                                  }));

            if (response.IsSuccessStatusCode)
            {
                var xml = await response.Content.ReadAsStringAsync();
                xml = "<?xml version='1.0' encoding='UTF-8'?><Info>" + xml.Replace('\n', ' ') + "</Info>";
                var serializer = new XmlSerializer(typeof(Info));
                using var reader = new StringReader(xml);
                info = (Info)serializer.Deserialize(reader)!;
            }
            else
            {
                info.Message = response.ReasonPhrase + "";
            }
        }
        catch (Exception ex) { info.Message = ex.Message; }

        return info!;
    }
}