namespace GolfManager.Shared.DTOs.Event;

public class MatchDetailResponse
{
    public string MatchupId { get; set; } = string.Empty;
    public int? StartingHole { get; set; }
    public int? StartingFlight { get; set; }
    public string? HomeTeamName { get; set; }
    public string? AwayTeamName { get; set; }
    public double? HomePoints { get; set; }
    public double? AwayPoints { get; set; }
    public List<MatchDetailMemberResponse> HomeMembers { get; set; } = new();
    public List<MatchDetailMemberResponse> AwayMembers { get; set; } = new();
    public List<MatchDetailHoleResponse> Holes { get; set; } = new();
}

public class MatchDetailMemberResponse
{
    public string SeasonGolferId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public double? Handicap { get; set; }
}

public class MatchDetailHoleResponse
{
    public int HoleNumber { get; set; }
    public int Par { get; set; }
    public int Yardage { get; set; }
    public int HoleHandicap { get; set; }
    public double? HomeNetUsed { get; set; }
    public double? AwayNetUsed { get; set; }
    public string HoleWinner { get; set; } = "None"; // "Home", "Away", "Tie", "None"
    public double HomeHolePoints { get; set; }
    public double AwayHolePoints { get; set; }
    public List<MatchDetailGolferScore> HomeScores { get; set; } = new();
    public List<MatchDetailGolferScore> AwayScores { get; set; } = new();
}

public class MatchDetailGolferScore
{
    public string SeasonGolferId { get; set; } = string.Empty;
    public int? GrossScore { get; set; }
    public int StrokesReceived { get; set; }
    public double? NetScore { get; set; }
    public bool IsUsed { get; set; }
}
