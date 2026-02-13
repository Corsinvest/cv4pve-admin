/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;

public class PveClientFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory) : IPveClientFactory
{
    public async Task<PveClient> CreateClientAsync(ClusterSettings settings, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient(settings.ValidateCertificate
                                                            ? "ProxmoxStrict"
                                                            : "ProxmoxIgnoreCert");

        var useApiToken = settings.AccessType == ClusterAccessType.ApiToken;

        return await ClientHelper.GetClientAndTryLoginAsync(settings.ApiHostsAndPortHA,
                                                            (host, port) => new PveClientWithRetry(host, port, httpClient, loggerFactory.CreateLogger<PveClientWithRetry>())
                                                            {
                                                                Username = useApiToken ? string.Empty : settings.ApiCredential.Username,
                                                                Password = useApiToken ? string.Empty : settings.ApiCredential.Password
                                                            },
                                                            useApiToken ? string.Empty : settings.ApiCredential.Username,
                                                            useApiToken ? string.Empty : settings.ApiCredential.Password,
                                                            useApiToken ? settings.ApiToken : string.Empty,
                                                            settings.ValidateCertificate,
                                                            loggerFactory,
                                                            settings.Timeout,
                                                            cancellationToken);
    }
}
