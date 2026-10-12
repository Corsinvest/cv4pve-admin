/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Collections.Concurrent;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Security.Services;

// Counts the requests of a key in a time window, in memory: the application runs as a single instance
public sealed class RequestLimiter
{
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _requests = new(StringComparer.Ordinal);

    // False when the key already made "limit" requests in the window. A refused request is not counted.
    public bool TryAcquire(string key, int limit, TimeSpan window)
    {
        var now = DateTime.UtcNow;
        var queue = _requests.GetOrAdd(key, _ => new());

        lock (queue)
        {
            while (queue.Count > 0 && now - queue.Peek() >= window) { queue.Dequeue(); }
            if (queue.Count >= limit) { return false; }

            queue.Enqueue(now);
        }

        // Keys that are no longer used must not pile up
        if (_requests.Count > 10000)
        {
            foreach (var item in _requests.Where(a => a.Value.Count == 0 || now - a.Value.Last() >= window).ToList())
            {
                _requests.TryRemove(item.Key, out _);
            }
        }

        return true;
    }
}
