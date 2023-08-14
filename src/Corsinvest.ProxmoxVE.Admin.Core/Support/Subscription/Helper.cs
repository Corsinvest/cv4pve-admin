/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Options;
using System.Xml.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Support.Subscription;

internal class Helper
{
    public static bool IsOkSubscriptions(Dictionary<ClusterNodeOptions, Info> checks, ClusterOptions clusterOptions)
        => checks.Where(a => clusterOptions.Nodes.Contains(a.Key))
                 .Any(a => a.Value.Status == Status.Active);

    public static async Task<Dictionary<ClusterNodeOptions, Info>> CheckAsync(AdminOptions adminOptions)
    {
        var checks = new Dictionary<ClusterNodeOptions, Info>();

        foreach (var cluster in adminOptions.Clusters)
        {
            checks.AddRange(await CheckAsync(cluster));
        }

        return checks;
    }

    public static async Task<Dictionary<ClusterNodeOptions, Info>> CheckAsync(ClusterOptions clusterOptions)
    {
        var checks = new Dictionary<ClusterNodeOptions, Info>();

        foreach (var node in clusterOptions.Nodes)
        {
            checks.Add(node, await RegisterAsync(node.ServerId, node.SubscriptionId));
        }

        return checks;
    }

    public static async Task<Info> RegisterAsync(string serverId, string licenseKey)
    {
        var info = new Info() { Status = Status.Invalid };

        if (!string.IsNullOrWhiteSpace(serverId) || !string.IsNullOrWhiteSpace(licenseKey))
        {
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
        }

        return info!;
    }
}