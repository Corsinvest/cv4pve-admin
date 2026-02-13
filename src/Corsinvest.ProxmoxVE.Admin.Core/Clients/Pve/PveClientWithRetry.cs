/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net;

namespace Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;

internal class PveClientWithRetry(string host,
                                  int port,
                                  HttpClient httpClient,
                                  ILogger<PveClientWithRetry> logger) : PveClient(host, port, httpClient)
{
    private bool _inLogin;

    public int MaxRetries { get; set; } = 3;
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
    public bool UseExponentialBackoff { get; set; } = true;

    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;

    protected override async Task<Result> ExecuteRequestAsync(string resource,
                                                              MethodType methodType,
                                                              IDictionary<string, object> parameters = null!)
    {
        if (_inLogin) { return await base.ExecuteRequestAsync(resource, methodType, parameters); }

        Result lastResult = null!;
        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var result = await base.ExecuteRequestAsync(resource, methodType, parameters);
                lastResult = result;

                // If request succeeded, return the result
                if (result.IsSuccessStatusCode
                    || (result.StatusCode == HttpStatusCode.InternalServerError && result.ResponseToDictionary.Count > 0))
                {
                    if (attempt > 0)
                    {
                        logger?.LogInformation("Request {Method} {Resource} succeeded on attempt {Attempt}", methodType, resource, attempt + 1);
                    }
                    return result;
                }

                // If we shouldn't retry, return the result as is
                if (!ShouldRetry(result) || attempt >= MaxRetries)
                {
                    logger?.LogWarning("Request {Method} {Resource} failed permanently: {StatusCode} - {ReasonPhrase}",
                                       methodType, resource, result.StatusCode, result.ReasonPhrase);
                    return result;
                }

                // If it's an authentication error, try to re-login
                if (IsAuthenticationError(result))
                {
                    logger?.LogWarning("Authentication error detected, attempting re-login...");
                    await TryReLoginAsync();
                }

                //            // Calculate delay before next attempt
                //            var delay = CalculateDelay(attempt);
                //            logger?.LogWarning("Attempt {Attempt} failed: {StatusCode} - {ReasonPhrase}. Retrying in {Delay}ms",
                //                attempt + 1, result.StatusCode, result.ReasonPhrase, delay.TotalMilliseconds);

                //            await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during request {Method} {Resource} (attempt {Attempt})", methodType, resource, attempt + 1);

                if (attempt >= MaxRetries) { throw; }

                var delay = CalculateDelay(attempt);
                await Task.Delay(delay);
            }
        }

        return lastResult ?? new Result(new { errors = "Max retries exceeded" },
                                        HttpStatusCode.InternalServerError,
                                        "Max retries exceeded",
                                        false,
                                        resource,
                                        parameters ?? new Dictionary<string, object>(),
                                        methodType,
                                        ResponseType);
    }

    private TimeSpan CalculateDelay(int attempt)
    {
        if (!UseExponentialBackoff) { return BaseDelay; }

        // Exponential backoff: 1s, 2s, 4s, 8s...
        var multiplier = Math.Pow(2, attempt);
        var delay = TimeSpan.FromMilliseconds(BaseDelay.TotalMilliseconds * multiplier);

        // Cap at 30 seconds maximum
        return delay > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : delay;
    }

    private static bool ShouldRetry(Result result)
    {
        if (result == null) { return true; }

        // Retry for network/connection errors
        var networkErrors = new[]
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout,
            (HttpStatusCode)0 // Connection error
        };

        if (networkErrors.Contains(result.StatusCode)) { return true; }

        // Retry for authentication errors (if we can re-authenticate)
        if (IsAuthenticationError(result)) { return true; }

        // Retry for temporary server errors
        if (result.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.TooManyRequests)
        {
            return true;
        }

        // Don't retry for client errors (4xx except auth)
        var statusCodeInt = (int)result.StatusCode;
        return statusCodeInt >= 400 && statusCodeInt < 500 && !IsAuthenticationError(result) ? false : false;
    }

    private static bool IsAuthenticationError(Result result)
        => result?.StatusCode == HttpStatusCode.Unauthorized ||
           result?.StatusCode == HttpStatusCode.Forbidden ||
           result?.ReasonPhrase?.Contains("authentication", StringComparison.OrdinalIgnoreCase) == true ||
           result?.ReasonPhrase?.Contains("token", StringComparison.OrdinalIgnoreCase) == true;

    private async Task<bool> TryReLoginAsync()
    {
        _inLogin = true;
        try
        {
            logger?.LogInformation("Attempting re-authentication...");

            bool loginSuccess;
            if (!string.IsNullOrEmpty(ApiToken))
            {
                var versionResult = await Version.Version();
                loginSuccess = versionResult.IsSuccessStatusCode;

                if (!loginSuccess)
                {
                    logger?.LogWarning("API token test failed: {StatusCode} - {ReasonPhrase}", versionResult.StatusCode, versionResult.ReasonPhrase);
                }
            }
            else
            {
                loginSuccess = await LoginAsync(Username, Password);
                if (!loginSuccess)
                {
                    logger?.LogWarning("Username/password login failed: {ReasonPhrase}", LastResult?.ReasonPhrase);
                }
            }

            if (loginSuccess) { logger?.LogInformation("Re-authentication completed successfully"); }

            return loginSuccess;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error during re-authentication");
            return false;
        }
        finally
        {
            _inLogin = false;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var result = await Version.Version();
            return result.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Connection test failed");
            return false;
        }
    }
}
