/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Collections.Concurrent;
using Corsinvest.ProxmoxVE.Admin.Core.Models;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wangkanai.Detection.Services;

namespace Corsinvest.ProxmoxVE.Admin.Module.Profile.Session;

internal class SessionInfoCircuitHandler(IServiceScopeFactory serviceScopeFactory, ILogger<SessionInfoCircuitHandler> logger)
    : CircuitHandler, ISessionsInfoTracker
{
    private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();

    public event EventHandler OnChanged = default!;
    public IEnumerable<SessionInfo> Sessions => _sessions.Values;

    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var session = GetSessionInfo(circuit);
        if (!string.IsNullOrEmpty(session?.CircuitId))
        {
            if (_sessions.TryGetValue(session.CircuitId, out var existingSession))
            {
                existingSession.Status = SessionStatus.Online;
                OnChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _sessions[session.CircuitId] = session;
                OnChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        await base.OnConnectionUpAsync(circuit, cancellationToken);
    }

    public override async Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var session = GetSessionInfo(circuit);
        if (session != null && !string.IsNullOrEmpty(session.CircuitId))
        {
            if (_sessions.TryGetValue(session.CircuitId, out var existingSession))
            {
                existingSession.Status = SessionStatus.TemporarilyOffline;
                OnChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        await base.OnConnectionDownAsync(circuit, cancellationToken);
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
        if (httpContextAccessor?.HttpContext != null)
        {
            httpContextAccessor.HttpContext.Items["CircuitId"] = circuit.Id;
            logger.LogDebug("Circuit opened and saved to HttpContext.Items: {CircuitId}", circuit.Id);
        }

        await base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_sessions.TryRemove(circuit.Id, out var removedSession))
        {
            removedSession.Status = SessionStatus.Offline;
            OnChanged?.Invoke(this, EventArgs.Empty);
            logger.LogDebug("Session removed for closed circuit: {CircuitId}", circuit.Id);
        }

        await base.OnCircuitClosedAsync(circuit, cancellationToken);
    }

    public void SetHubConnectionId(string circuitId, string hubConnectionId)
    {
        if (_sessions.TryGetValue(circuitId, out var session))
        {
            session.HubConnectionId = hubConnectionId;
            OnChanged?.Invoke(this, EventArgs.Empty);
            logger.LogDebug("Hub connection ID set for circuit {CircuitId}: {HubConnectionId}",
                circuitId, hubConnectionId);
        }
        else
        {
            logger.LogWarning("Circuit not found when setting HubConnectionId: {CircuitId}", circuitId);
        }
    }

    public void UpdateCurrentPage(string circuitId, string currentPage)
    {
        if (_sessions.TryGetValue(circuitId, out var session))
        {
            session.CurrentPage = currentPage;
            session.LastActivity = DateTime.UtcNow;
            OnChanged?.Invoke(this, EventArgs.Empty);
            logger.LogDebug("Current page updated for circuit {CircuitId}: {CurrentPage}",
                circuitId, currentPage);
        }
    }

    private SessionInfo GetSessionInfo(Circuit circuit)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var detection = scope.ServiceProvider.GetService<IDetectionService>()!;
        var currentUser = scope.ServiceProvider.GetService<ICurrentUserService>()!;

        return new()
        {
            HttpConnectionId = currentUser.HttpConnectionId,
            CircuitId = circuit.Id,
            Status = SessionStatus.Online,
            UserName = currentUser.UserName,
            IpAddress = currentUser.IpAddress,
            Browser = detection.Browser.Name.ToString(),
            BrowserVersion = detection.Browser.Version,
            Platform = detection.Platform.Name.ToString(),
            PlatformVersion = detection.Platform.Version,
            Device = detection.Device.Type.ToString(),
            UserAgent = detection.UserAgent.ToString(),
            Login = DateTime.UtcNow
        };
    }
}
