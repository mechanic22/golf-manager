using GolfManager.Core.Enums;
using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Shared.Templates;

/// <summary>
/// Predefined season settings templates
/// </summary>
public static class SeasonSettingsTemplates
{
    /// <summary>
    /// Standard 18-hole league with Bob's handicap system
    /// </summary>
    public static UpdateSeasonSettingsRequest StandardEighteenHole => new()
    {
        HandicapType = HandicapType.Bobs,
        MaxHandicap = 18,
        MaxScoreForHandicap = MaxScoreForHandicap.PlusFour,
        IndividualScoringType = IndividualScoringType.TwoPoint,
        TeamScoringType = TeamScoringType.MatchPoints,
        MissingPlayerType = MissingPlayerType.FieldAverage,
        MissingTeamType = MissingTeamType.PartialPoints,
        DefaultStartTime = new TimeOnly(17, 30) // 5:30 PM
    };

    /// <summary>
    /// Nine-hole evening league
    /// </summary>
    public static UpdateSeasonSettingsRequest NineHoleEvening => new()
    {
        HandicapType = HandicapType.Bobs,
        MaxHandicap = 9,
        MaxScoreForHandicap = MaxScoreForHandicap.PlusFour,
        IndividualScoringType = IndividualScoringType.TwoPoint,
        TeamScoringType = TeamScoringType.MatchPoints,
        MissingPlayerType = MissingPlayerType.FieldAverage,
        MissingTeamType = MissingTeamType.PartialPoints,
        DefaultStartTime = new TimeOnly(18, 0) // 6:00 PM
    };

    /// <summary>
    /// Competitive league with USGA handicap system
    /// </summary>
    public static UpdateSeasonSettingsRequest CompetitiveUSGA => new()
    {
        HandicapType = HandicapType.USGA,
        MaxHandicap = 36,
        MaxScoreForHandicap = MaxScoreForHandicap.DoublePar,
        IndividualScoringType = IndividualScoringType.Stableford,
        TeamScoringType = TeamScoringType.MatchPoints,
        MissingPlayerType = MissingPlayerType.None,
        MissingTeamType = MissingTeamType.NoPoints,
        DefaultStartTime = new TimeOnly(8, 0) // 8:00 AM
    };

    /// <summary>
    /// Casual league with simple scoring
    /// </summary>
    public static UpdateSeasonSettingsRequest CasualLeague => new()
    {
        HandicapType = HandicapType.Scratch,
        MaxHandicap = 18,
        MaxScoreForHandicap = MaxScoreForHandicap.PlusFour,
        IndividualScoringType = IndividualScoringType.TwoPoint,
        TeamScoringType = TeamScoringType.MatchPoints,
        MissingPlayerType = MissingPlayerType.FieldAverage,
        MissingTeamType = MissingTeamType.PartialPoints,
        DefaultStartTime = new TimeOnly(17, 0) // 5:00 PM
    };

    /// <summary>
    /// Stableford scoring league
    /// </summary>
    public static UpdateSeasonSettingsRequest Stableford => new()
    {
        HandicapType = HandicapType.Bobs,
        MaxHandicap = 18,
        MaxScoreForHandicap = MaxScoreForHandicap.PlusFour,
        IndividualScoringType = IndividualScoringType.Stableford,
        TeamScoringType = TeamScoringType.MatchPoints,
        MissingPlayerType = MissingPlayerType.PlayAgainstPar,
        MissingTeamType = MissingTeamType.NoPoints,
        DefaultStartTime = new TimeOnly(17, 30) // 5:30 PM
    };

    /// <summary>
    /// Scratch (no handicap) league
    /// </summary>
    public static UpdateSeasonSettingsRequest ScratchLeague => new()
    {
        HandicapType = HandicapType.Scratch,
        MaxHandicap = 0,
        MaxScoreForHandicap = MaxScoreForHandicap.None,
        IndividualScoringType = IndividualScoringType.TwoPoint,
        TeamScoringType = TeamScoringType.MatchPoints,
        MissingPlayerType = MissingPlayerType.BlindDraw,
        MissingTeamType = MissingTeamType.NoPoints,
        DefaultStartTime = new TimeOnly(17, 30) // 5:30 PM
    };

    /// <summary>
    /// Get all available templates
    /// </summary>
    public static Dictionary<string, TemplateInfo> GetAllTemplates()
    {
        return new Dictionary<string, TemplateInfo>
        {
            ["standard-18"] = new TemplateInfo
            {
                Key = "standard-18",
                Name = "Standard 18-Hole League",
                Description = "Traditional 18-hole league with Bob's Famous handicap system and two-point scoring",
                Settings = StandardEighteenHole
            },
            ["nine-hole-evening"] = new TemplateInfo
            {
                Key = "nine-hole-evening",
                Name = "Nine-Hole Evening League",
                Description = "Quick 9-hole format perfect for weeknight play",
                Settings = NineHoleEvening
            },
            ["competitive-usga"] = new TemplateInfo
            {
                Key = "competitive-usga",
                Name = "Competitive USGA League",
                Description = "Serious competition using official USGA handicap system",
                Settings = CompetitiveUSGA
            },
            ["casual"] = new TemplateInfo
            {
                Key = "casual",
                Name = "Casual League",
                Description = "Relaxed format with simple handicapping and scoring",
                Settings = CasualLeague
            },
            ["stableford"] = new TemplateInfo
            {
                Key = "stableford",
                Name = "Stableford Scoring League",
                Description = "Points-based Stableford scoring system",
                Settings = Stableford
            },
            ["scratch"] = new TemplateInfo
            {
                Key = "scratch",
                Name = "Scratch League",
                Description = "No handicaps - pure skill competition",
                Settings = ScratchLeague
            }
        };
    }
}

/// <summary>
/// Template information
/// </summary>
public class TemplateInfo
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public UpdateSeasonSettingsRequest Settings { get; set; } = new();
}

