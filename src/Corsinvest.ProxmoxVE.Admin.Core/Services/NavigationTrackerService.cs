/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Security.Identity;
using Microsoft.AspNetCore.Components.Routing;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class NavigationTrackerService : IDisposable
{
    private readonly NavigationManager _navigationManager;
    private readonly ISessionsInfoTracker _sessionsInfoTracker;
    private readonly ICurrentUserService _currentUserService;

    public NavigationTrackerService(NavigationManager navigationManager,
                                    ISessionsInfoTracker sessionsInfoTracker,
                                    ICurrentUserService currentUserService)
    {
        _navigationManager = navigationManager;
        _sessionsInfoTracker = sessionsInfoTracker;
        _currentUserService = currentUserService;

        _navigationManager.LocationChanged += OnLocationChanged;

        UpdateCurrentPage(_navigationManager.Uri);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => UpdateCurrentPage(e.Location);

    private void UpdateCurrentPage(string uri)
    {
        var circuitId = _currentUserService.CircuitId;
        if (!string.IsNullOrEmpty(circuitId))
        {
            var relativePath = _navigationManager.ToBaseRelativePath(uri);
            _sessionsInfoTracker.UpdateCurrentPage(circuitId, $"/{relativePath}");
        }
    }

    public void Dispose() => _navigationManager.LocationChanged -= OnLocationChanged;
}
