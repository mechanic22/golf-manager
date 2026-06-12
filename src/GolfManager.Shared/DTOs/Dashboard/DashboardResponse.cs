namespace GolfManager.Shared.DTOs.Dashboard;

public class DashboardResponse
{
    public UserSummary User { get; set; } = new();
    public List<DashboardLeagueItem> Leagues { get; set; } = [];

    public class UserSummary
    {
        public string FirstName { get; set; } = string.Empty;
        public double? HandicapIndex { get; set; }
        public int RoundsCount { get; set; }
    }

    public class DashboardLeagueItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public bool IsAdmin { get; set; }
        public ActiveSeasonSummary? ActiveSeason { get; set; }
    }

    public class ActiveSeasonSummary
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public NextEventSummary? NextEvent { get; set; }
    }

    public class NextEventSummary
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public DateTime EventDate { get; set; }
        public bool NeedsScores { get; set; }
    }
}
