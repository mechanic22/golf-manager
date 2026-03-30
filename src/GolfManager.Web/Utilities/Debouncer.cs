using System.Collections.Concurrent;

namespace GolfManager.Web.Utilities;

/// <summary>
/// Utility for debouncing actions to prevent excessive API calls
/// </summary>
public static class Debouncer
{
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _tokens = new();

    /// <summary>
    /// Debounce an action by key. If called multiple times with the same key,
    /// only the last action will execute after the delay period.
    /// </summary>
    /// <param name="key">Unique key for this debounced action</param>
    /// <param name="action">Action to execute after delay</param>
    /// <param name="delayMs">Delay in milliseconds (default: 1000ms)</param>
    public static void Debounce(string key, Action action, int delayMs = 1000)
    {
        var tokenSource = _tokens.AddOrUpdate(key,
            k => new CancellationTokenSource(),
            (k, cts) =>
            {
                cts.Cancel();
                return new CancellationTokenSource();
            });

        Task.Delay(delayMs, tokenSource.Token).ContinueWith(task =>
        {
            if (!task.IsCanceled)
            {
                action();
                if (_tokens.TryRemove(key, out var cts))
                {
                    cts.Dispose();
                }
            }
        }, tokenSource.Token);
    }
}

