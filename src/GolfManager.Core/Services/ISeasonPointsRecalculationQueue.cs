namespace GolfManager.Core.Services;

public interface ISeasonPointsRecalculationQueue
{
    ValueTask QueueSeasonAsync(string leagueId, string seasonId, string requestedBy, CancellationToken cancellationToken = default);

    ValueTask<SeasonPointsRecalculationWorkItem> DequeueAsync(CancellationToken cancellationToken);
}

public sealed record SeasonPointsRecalculationWorkItem(
    string LeagueId,
    string SeasonId,
    string RequestedBy);
