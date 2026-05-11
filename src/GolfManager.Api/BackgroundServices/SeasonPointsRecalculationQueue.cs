using System.Collections.Concurrent;
using System.Threading.Channels;
using GolfManager.Core.Services;

namespace GolfManager.Api.BackgroundServices;

public sealed class SeasonPointsRecalculationQueue : ISeasonPointsRecalculationQueue
{
    private readonly Channel<SeasonPointsRecalculationWorkItem> _channel = Channel.CreateUnbounded<SeasonPointsRecalculationWorkItem>();
    private readonly ConcurrentDictionary<string, byte> _pendingKeys = new(StringComparer.OrdinalIgnoreCase);

    public ValueTask QueueSeasonAsync(string leagueId, string seasonId, string requestedBy, CancellationToken cancellationToken = default)
    {
        var workItem = new SeasonPointsRecalculationWorkItem(leagueId, seasonId, requestedBy);
        var key = BuildKey(workItem);
        if (!_pendingKeys.TryAdd(key, 0))
        {
            return ValueTask.CompletedTask;
        }

        return _channel.Writer.WriteAsync(workItem, cancellationToken);
    }

    public async ValueTask<SeasonPointsRecalculationWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _channel.Reader.ReadAsync(cancellationToken);
        _pendingKeys.TryRemove(BuildKey(workItem), out _);
        return workItem;
    }

    private static string BuildKey(SeasonPointsRecalculationWorkItem workItem)
    {
        return $"season:{workItem.LeagueId}:{workItem.SeasonId}";
    }
}
