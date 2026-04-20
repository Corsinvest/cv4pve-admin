/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class ParallelHelper
{
    public const int DefaultMaxParallelism = 5;

    /// <summary>
    /// Runs an async function on each element with limited parallelism.
    /// </summary>
    public static async Task<IEnumerable<TResult>> RunAsync<T, TResult>(
        IEnumerable<T> source,
        Func<T, Task<TResult>> func,
        int maxParallelism = DefaultMaxParallelism)
    {
        var semaphore = new SemaphoreSlim(maxParallelism);
        var tasks = source.Select(async item =>
        {
            await semaphore.WaitAsync();
            try { return await func(item); }
            finally { semaphore.Release(); }
        });
        return await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Runs an async function that returns a collection on each element,
    /// flattening all results into a single sequence.
    /// </summary>
    public static async Task<IEnumerable<TResult>> RunManyAsync<T, TResult>(
        IEnumerable<T> source,
        Func<T, Task<IEnumerable<TResult>>> func,
        int maxParallelism = DefaultMaxParallelism)
        => (await RunAsync(source, func, maxParallelism)).SelectMany(a => a);
}
