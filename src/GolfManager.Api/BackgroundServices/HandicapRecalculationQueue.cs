using System.Collections.Concurrent;
using System.Threading.Channels;
using GolfManager.Core.Services;

namespace GolfManager.Api.BackgroundServices;

public sealed class HandicapRecalculationQueue : IHandicapRecalculationQueue
{
    private readonly Channel<HandicapRecalculationWorkItem> _channel = Channel.CreateUnbounded<HandicapRecalculationWorkItem>();
    private readonly ConcurrentDictionary<string, byte> _pendingKeys = new(StringComparer.OrdinalIgnoreCase);

    public ValueTask QueueEventAsync(string leagueId, string seasonId, string eventId, string requestedBy, CancellationToken cancellationToken = default)
    {
        var workItem = new HandicapRecalculationWorkItem(leagueId, seasonId, eventId, null, requestedBy);
        return QueueInternalAsync(workItem, cancellationToken);
    }

    public ValueTask QueueGolferAsync(string leagueId, string seasonId, string eventId, string golferId, string requestedBy, CancellationToken cancellationToken = default)
    {
        var workItem = new HandicapRecalculationWorkItem(leagueId, seasonId, eventId, golferId, requestedBy);
        return QueueInternalAsync(workItem, cancellationToken);
    }

    public async ValueTask<HandicapRecalculationWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _channel.Reader.ReadAsync(cancellationToken);
        _pendingKeys.TryRemove(BuildKey(workItem), out _);
        return workItem;
    }

    private ValueTask QueueInternalAsync(HandicapRecalculationWorkItem workItem, CancellationToken cancellationToken)
    {
        var key = BuildKey(workItem);
        if (!_pendingKeys.TryAdd(key, 0))
        {
            return ValueTask.CompletedTask;
        }

        return _channel.Writer.WriteAsync(workItem, cancellationToken);
    }

    private static string BuildKey(HandicapRecalculationWorkItem workItem)
    {
        return string.IsNullOrWhiteSpace(workItem.GolferId)
            ? $"event:{workItem.LeagueId}:{workItem.SeasonId}:{workItem.EventId}"
            : $"golfer:{workItem.LeagueId}:{workItem.SeasonId}:{workItem.EventId}:{workItem.GolferId}";
    }
}
