namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

/// <summary>
/// Helper for running async methods synchronously
/// </summary>
public static class AsyncHelper
{
    /// <summary>
    /// Execute an async method synchronously
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="func">Async function to execute</param>
    /// <returns>Result of the async operation</returns>
    public static T RunSync<T>(Func<Task<T>> func) => Task.Run(() => func()).GetAwaiter().GetResult();

    /// <summary>
    /// Execute an async method synchronously (void return)
    /// </summary>
    /// <param name="func">Async function to execute</param>
    public static void RunSync(Func<Task> func) => Task.Run(() => func()).GetAwaiter().GetResult();
}
