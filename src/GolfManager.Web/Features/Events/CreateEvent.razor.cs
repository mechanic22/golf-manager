using GolfManager.Core.Enums;
using GolfManager.Shared.DTOs.OneTimeEvent;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace GolfManager.Web.Features.Events;

public partial class CreateEvent : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private IOneTimeEventService EventService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<CreateEvent> Logger { get; set; } = null!;

    private bool canCreate = false;
    private bool isSubmitting = false;
    private string? errorMessage;

    private readonly CreateEventModel model = new();

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        canCreate = AuthorizationService.IsGlobalAdmin();
        await Task.CompletedTask;
    }

    private async Task HandleValidSubmit()
    {
        if (isSubmitting) return;

        try
        {
            isSubmitting = true;
            errorMessage = null;

            var eventDateTime = CombineDateAndTime(model.EventDate, model.TeeTimeString);
            var key = GenerateKey(model.EventName);

            var request = new CreateOneTimeEventRequest
            {
                Key = key,
                Name = model.EventName,
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description,
                EventDate = eventDateTime,
                Format = MapFormat(model.Format),
                TeamSize = model.Format == "individual" ? 1 : 4,
                MaxTeams = model.MaxPlayers.HasValue ? (int?)Math.Ceiling(model.MaxPlayers.Value / 4.0) : null,
                RegistrationDeadline = model.RegistrationDeadline,
                AccessType = EventAccessType.Public,
                HolesPlayed = HolesPlayed.Eighteen,
                TotalRounds = 1
            };

            var response = await EventService.CreateEventAsync(request);
            if (response?.Success == true && response.Data != null)
            {
                Navigation.NavigateTo($"/organizer/{response.Data.Key}");
            }
            else
            {
                errorMessage = response?.Message ?? "Failed to create event. Please try again.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to create event: {ex.Message}";
            Logger.LogError(ex, "Error creating event");
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private static DateTime CombineDateAndTime(DateTime date, string? timeString)
    {
        if (!string.IsNullOrWhiteSpace(timeString) && TimeSpan.TryParse(timeString, out var time))
            return date.Date + time;
        return date.Date.AddHours(9);
    }

    private static string GenerateKey(string name)
    {
        var key = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");
        key = System.Text.RegularExpressions.Regex.Replace(key, @"[^a-z0-9-]", "");
        key = System.Text.RegularExpressions.Regex.Replace(key, @"-{2,}", "-").Trim('-');
        return key.Length > 50 ? key[..50] : key;
    }

    private static ScoringFormat MapFormat(string format) => format switch
    {
        "scramble" => ScoringFormat.Scramble,
        "bestball" => ScoringFormat.BestBall,
        "team" => ScoringFormat.MatchPlay,
        _ => ScoringFormat.StrokePlay
    };

    public class CreateEventModel
    {
        [Required(ErrorMessage = "Event name is required")]
        [MaxLength(100, ErrorMessage = "Event name must be 100 characters or fewer")]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description must be 500 characters or fewer")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required")]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);

        public string? TeeTimeString { get; set; } = "09:00";

        [Required(ErrorMessage = "Golf course is required")]
        [MaxLength(150)]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        public string Format { get; set; } = "individual";

        [Range(1, 999, ErrorMessage = "Maximum players must be between 1 and 999")]
        public int? MaxPlayers { get; set; }

        public DateTime? RegistrationDeadline { get; set; }
    }
}
