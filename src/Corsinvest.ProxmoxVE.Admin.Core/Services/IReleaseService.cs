/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IReleaseService
{
    /// <summary>
    /// Event raised when a new release is discovered
    /// </summary>
    event EventHandler<ReleaseInfo>? NewReleaseDiscovered;

    /// <summary>
    /// Gets whether automatic updates are supported in the current environment
    /// </summary>
    bool IsAutoUpdateSupported { get; }

    /// <summary>
    /// Gets all available releases from the release source
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<IEnumerable<ReleaseInfo>> GetReleasesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a new release is available (cached for 12 hours per includePrerelease value)
    /// </summary>
    /// <param name="includePrerelease">Whether to include prerelease versions</param>
    /// <param name="force">Force cache refresh</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The latest release info if a newer version is available, null otherwise</returns>
    Task<ReleaseInfo?> NewReleaseIsAvailableAsync(bool includePrerelease = false, bool force = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers an update of the application
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if update was triggered successfully, false otherwise</returns>
    /// <exception cref="NotSupportedException">Thrown when update is not supported (e.g., GitHub releases)</exception>
    Task<bool> TriggerUpdateAsync(CancellationToken cancellationToken = default);
}
