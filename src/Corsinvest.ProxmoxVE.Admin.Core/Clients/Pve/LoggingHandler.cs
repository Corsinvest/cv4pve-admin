namespace Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;

internal class LoggingHandler(HttpMessageHandler innerHandler, ILogger logger) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        //logger.LogInformation("HTTP {Method} {Uri}", request.Method, request.RequestUri);

        //if (request.Content != null)
        //{
        //    var requestContent = await request.Content.ReadAsStringAsync();
        //    logger.LogInformation("Request Content: {body}", requestContent);
        //}
        _ = logger;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        stopwatch.Stop();

        //        logger.LogInformation("HTTP {Method} {Uri} completed in {ElapsedMs}ms with status {StatusCode}",
        //                        request.Method,
        //                        request.RequestUri,
        //                        stopwatch.ElapsedMilliseconds,
        //                        response.StatusCode);

        //if (response.Content != null)
        //{
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    logger.LogInformation("Response Content: {body}", responseContent);
        //}

        return response;
    }
}
