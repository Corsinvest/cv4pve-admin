/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net.Mime;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.Core.Common.Middleware;

internal class ExceptionHandlingMiddleware(RequestDelegate next,
                                           ILogger<ExceptionHandlingMiddleware> logger,
                                           IWebHostEnvironment environment)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred in the middleware.");

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = MediaTypeNames.Application.Json;

                object response = environment.IsDevelopment()
                    ? new { message = "An unexpected error occurred.", details = ex.Message, stackTrace = ex.StackTrace }
                    : new { message = "An unexpected error occurred." };

                await context.Response.WriteAsJsonAsync(response);
            }
            else
            {
                logger.LogWarning("The response has already started, the error response cannot be sent.");
            }
        }
    }
}
