using System.Collections.Concurrent;
using System.Threading.Channels;
using GolfManager.Core.Services;

namespace GolfManager.Api.BackgroundServices;

public sealed class SeasonPointsRecalculationQueue : ISeasonPointsRecalculationQueue
{
    private readonly Channel<string> _keys = Channel.CreateUnbounded<string>();
    private readonly ConcurrentDictionary<string, SeasonPointsRecalculationWorkItem> _pending =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(300);

    public ValueTask QueueSeasonAsync(string leagueId, string seasonId, string requestedBy, CancellationToken cancellationToken = default)
    {
        var item = new SeasonPointsRecalculationWorkItem(leagueId, seasonId, requestedBy);
        var key = BuildKey(item);
        var isNew = _pending.TryAdd(key, item);

        if (!isNew)
        {
            _pending[key] = item;
            return ValueTask.CompletedTask;
        }

        return _keys.Writer.WriteAsync(key, cancellationToken);
    }

    public async ValueTask<SeasonPointsRecalculationWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var key = await _keys.Reader.ReadAsync(cancellationToken);
            await Task.Delay(DebounceDelay, cancellationToken);

            if (_pending.TryRemove(key, out var item))
                return item;
        }
    }

    private static string BuildKey(SeasonPointsRecalculationWorkItem item) =>
        $"season:{item.LeagueId}:{item.SeasonId}";
}
