/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;

internal class LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        logger.LogInformation("HTTP {Method} {Uri}", request.Method, request.RequestUri);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        stopwatch.Stop();

        logger.LogInformation("HTTP {Method} {Uri} completed in {ElapsedMs}ms with status {StatusCode}",
                        request.Method,
                        request.RequestUri,
                        stopwatch.ElapsedMilliseconds,
                        response.StatusCode);

        return response;
    }
}
