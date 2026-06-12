using System.Collections.Concurrent;
using System.Threading.Channels;
using GolfManager.Core.Services;

namespace GolfManager.Api.BackgroundServices;

public sealed class HandicapRecalculationQueue : IHandicapRecalculationQueue
{
    // Channel carries keys only; latest work item is stored separately so rapid-fire
    // triggers coalesce — only one channel slot per key, always processes the newest item.
    private readonly Channel<string> _keys = Channel.CreateUnbounded<string>();
    private readonly ConcurrentDictionary<string, HandicapRecalculationWorkItem> _pending =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(300);

    public ValueTask QueueEventAsync(string leagueId, string seasonId, string eventId, string requestedBy, CancellationToken cancellationToken = default)
    {
        var item = new HandicapRecalculationWorkItem(leagueId, seasonId, eventId, null, requestedBy);
        return EnqueueAsync(item, cancellationToken);
    }

    public ValueTask QueueGolferAsync(string leagueId, string seasonId, string eventId, string golferId, string requestedBy, CancellationToken cancellationToken = default)
    {
        var item = new HandicapRecalculationWorkItem(leagueId, seasonId, eventId, golferId, requestedBy);
        return EnqueueAsync(item, cancellationToken);
    }

    public async ValueTask<HandicapRecalculationWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var key = await _keys.Reader.ReadAsync(cancellationToken);

            // Debounce: wait briefly so rapid-fire updates settle on the latest item.
            await Task.Delay(DebounceDelay, cancellationToken);

            // Now remove and return whatever the latest item is for this key.
            if (_pending.TryRemove(key, out var item))
                return item;

            // Item was already handled (shouldn't happen, but be safe and loop).
        }
    }

    private ValueTask EnqueueAsync(HandicapRecalculationWorkItem item, CancellationToken cancellationToken)
    {
        var key = BuildKey(item);
        var isNew = _pending.TryAdd(key, item);

        if (!isNew)
        {
            // Already a pending channel slot — just update to the latest item.
            _pending[key] = item;
            return ValueTask.CompletedTask;
        }

        return _keys.Writer.WriteAsync(key, cancellationToken);
    }

    private static string BuildKey(HandicapRecalculationWorkItem item) =>
        string.IsNullOrWhiteSpace(item.GolferId)
            ? $"event:{item.LeagueId}:{item.SeasonId}:{item.EventId}"
            : $"golfer:{item.LeagueId}:{item.SeasonId}:{item.EventId}:{item.GolferId}";
}
