namespace GolfManager.Core.Services;

public interface IHandicapRecalculationQueue
{
    ValueTask QueueEventAsync(string leagueId, string seasonId, string eventId, string requestedBy, CancellationToken cancellationToken = default);

    ValueTask QueueGolferAsync(string leagueId, string seasonId, string eventId, string golferId, string requestedBy, CancellationToken cancellationToken = default);

    ValueTask<HandicapRecalculationWorkItem> DequeueAsync(CancellationToken cancellationToken);
}

public sealed record HandicapRecalculationWorkItem(
    string LeagueId,
    string SeasonId,
    string EventId,
    string? GolferId,
    string RequestedBy);
