using GolfManager.Core.Enums;
using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Shared.Validation;

/// <summary>
/// Validator for season settings
/// </summary>
public static class SeasonSettingsValidator
{
    /// <summary>
    /// Validate season settings update request
    /// </summary>
    public static ValidationResult Validate(UpdateSeasonSettingsRequest request)
    {
        var errors = new List<string>();

        // Handicap validation
        if (request.HandicapType.HasValue && request.HandicapType != HandicapType.None)
        {
            if (!request.MaxHandicap.HasValue || request.MaxHandicap <= 0)
            {
                errors.Add("Max handicap is required when using a handicap system");
            }
            else if (request.MaxHandicap > 54)
            {
                errors.Add("Max handicap cannot exceed 54");
            }

            if (!request.MaxScoreForHandicap.HasValue || request.MaxScoreForHandicap == MaxScoreForHandicap.None)
            {
                errors.Add("Max score for handicap is required when using a handicap system");
            }
        }

        // Individual scoring validation
        if (request.IndividualScoringType.HasValue && request.IndividualScoringType == IndividualScoringType.None)
        {
            errors.Add("Individual scoring type must be specified");
        }

        // Team scoring validation
        if (request.TeamScoringType.HasValue && request.TeamScoringType == TeamScoringType.None)
        {
            errors.Add("Team scoring type must be specified");
        }

        // Missing player validation
        if (request.MissingPlayerType.HasValue && request.MissingPlayerType == MissingPlayerType.None)
        {
            errors.Add("Missing player handling must be specified");
        }

        // Business rule: If using team scoring, must specify missing team handling
        if (request.TeamScoringType.HasValue &&
            request.TeamScoringType != TeamScoringType.None &&
            !request.MissingTeamType.HasValue)
        {
            errors.Add("Missing team handling is required when using team scoring");
        }

        // Business rule: If using individual scoring, must specify missing player handling
        if (request.IndividualScoringType.HasValue && 
            request.IndividualScoringType != IndividualScoringType.None &&
            (!request.MissingPlayerType.HasValue || request.MissingPlayerType == MissingPlayerType.None))
        {
            errors.Add("Missing player handling is required when using individual scoring");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    /// <summary>
    /// Validate that settings are complete for season activation
    /// </summary>
    public static ValidationResult ValidateForActivation(SeasonSettingsResponse settings)
    {
        var errors = new List<string>();

        if (settings.HandicapType == HandicapType.None)
        {
            errors.Add("Handicap type must be configured before activating season");
        }

        if (settings.IndividualScoringType == IndividualScoringType.None)
        {
            errors.Add("Individual scoring type must be configured before activating season");
        }

        if (settings.TeamScoringType == TeamScoringType.None)
        {
            errors.Add("Team scoring type must be configured before activating season");
        }

        if (!settings.MaxHandicap.HasValue || settings.MaxHandicap <= 0)
        {
            errors.Add("Max handicap must be set before activating season");
        }

        if (settings.MaxScoreForHandicap == MaxScoreForHandicap.None)
        {
            errors.Add("Max score for handicap must be configured before activating season");
        }

        if (settings.MissingPlayerType == MissingPlayerType.None)
        {
            errors.Add("Missing player handling must be configured before activating season");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

