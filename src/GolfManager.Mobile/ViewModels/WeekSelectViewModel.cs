using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GolfManager.Mobile.Services;
using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.League;
using System.Collections.ObjectModel;

namespace GolfManager.Mobile.ViewModels;

public partial class WeekSelectViewModel : ObservableObject, IQueryAttributable
{
    private readonly IEventService _eventService;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    private LeagueResponse? _league;

    public ObservableCollection<EventResponse> Events { get; } = new();

    public WeekSelectViewModel(IEventService eventService) => _eventService = eventService;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("league", out var l) && l is LeagueResponse league)
        {
            _league = league;
            _ = LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        if (_league?.ActiveSeasonId == null) return;
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var result = await _eventService.GetEventsAsync(_league.ActiveSeasonId, _league.Key);
            Events.Clear();
            foreach (var e in result?.Items ?? []) Events.Add(e);
        }
        catch { ErrorMessage = "Failed to load events."; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task SelectEventAsync(EventResponse evt)
    {
        try
        {
            await Shell.Current.GoToAsync("hole", new Dictionary<string, object>
            {
                ["event"] = evt,
                ["league"] = _league!
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Navigation failed: {ex.Message}";
        }
    }
}
