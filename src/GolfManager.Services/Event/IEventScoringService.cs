using GolfManager.Core.Entities;
using GolfManager.Shared.DTOs.Event;

namespace GolfManager.Services.Event;

public interface IEventScoringService
{
    Task<EventScoreboardResponse> BuildEventScoreboardAsync(SeasonEvent seasonEvent, string leagueId);
    Task PersistEventScoreboardAsync(string eventId, string leagueId, EventScoreboardResponse scoreboard);
    Task<EventScoreboardResponse?> TryBuildFromStoredAsync(SeasonEvent seasonEvent, string leagueId);
    Task<int> RecalculateSeasonTeamStandingsAsync(string seasonId, string leagueId, string userId);
    Task<MatchDetailResponse?> BuildMatchDetailAsync(string matchupId, SeasonEvent seasonEvent, string leagueId);
}
