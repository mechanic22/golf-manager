using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GolfManager.Mobile.Models;
using GolfManager.Mobile.Services;
using GolfManager.Shared.DTOs.Course;
using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.League;
using System.Collections.ObjectModel;

namespace GolfManager.Mobile.ViewModels;

public partial class HoleViewModel : ObservableObject, IQueryAttributable
{
    private readonly IGpsService _gps;
    private readonly DistanceService _distance;
    private readonly ICourseService _courseService;

    [ObservableProperty] private bool _isDistanceTab = true;
    public bool IsScoresTab => !IsDistanceTab;
    partial void OnIsDistanceTabChanged(bool value) => OnPropertyChanged(nameof(IsScoresTab));

    [RelayCommand] private void ShowDistance() => IsDistanceTab = true;
    [RelayCommand] private void ShowScores() => IsDistanceTab = false;

    [ObservableProperty] private string _holeTitle = "Hole 1";
    [ObservableProperty] private string _holeSubtitle = string.Empty;
    [ObservableProperty] private int _currentHoleNumber = 1;
    [ObservableProperty] private double _distanceFront;
    [ObservableProperty] private double _distanceCenter;
    [ObservableProperty] private double _distanceBack;
    [ObservableProperty] private string _gpsStatusText = "Acquiring GPS...";
    [ObservableProperty] private bool _isSaving;

    public ObservableCollection<PlayerScore> PlayerScores { get; } = new();

    private IList<HoleGpsResponse> _holes = [];
    private HoleGpsResponse? _currentHoleGps;
    private string? _leagueKey;

    public HoleViewModel(IGpsService gps, DistanceService distance, ICourseService courseService)
    {
        _gps = gps;
        _distance = distance;
        _courseService = courseService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("league", out var l) && l is LeagueResponse league)
            _leagueKey = league.Key;

        if (query.TryGetValue("event", out var e) && e is EventResponse evt && evt.CourseId != null)
            _ = LoadHolesAsync(evt.CourseId);
    }

    private async Task LoadHolesAsync(string courseId)
    {
        _holes = await _courseService.GetHoleGpsAsync(courseId) ?? [];
        if (_holes.Count > 0)
        {
            _currentHoleGps = _holes[0];
            UpdateHoleDisplay();
        }
    }

    public Task StartGpsAsync()
    {
        _gps.LocationUpdated += OnLocationUpdated;
        return _gps.StartAsync();
    }

    public void StopGps() => _gps.LocationUpdated -= OnLocationUpdated;

    private void OnLocationUpdated(Location loc)
    {
        var detected = _gps.DetectCurrentHole(_holes);
        if (detected != null && detected.HoleNumber != CurrentHoleNumber)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _currentHoleGps = detected;
                CurrentHoleNumber = detected.HoleNumber;
                UpdateHoleDisplay();
            });
        }

        if (_currentHoleGps?.GreenLatitude.HasValue == true
            && _currentHoleGps.GreenLongitude.HasValue
            && _currentHoleGps.GreenRadius.HasValue)
        {
            var (front, center, back) = DistanceService.GreenDistances(
                loc.Latitude, loc.Longitude,
                _currentHoleGps.GreenLatitude.Value,
                _currentHoleGps.GreenLongitude.Value,
                _currentHoleGps.GreenRadius.Value);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DistanceFront = Math.Round(front, 0);
                DistanceCenter = Math.Round(center, 0);
                DistanceBack = Math.Round(back, 0);
                GpsStatusText = $"GPS ±{loc.Accuracy:0}m";
            });
        }
    }

    private void UpdateHoleDisplay()
    {
        HoleTitle = $"Hole {CurrentHoleNumber}";
    }

    [RelayCommand]
    private void PreviousHole()
    {
        if (CurrentHoleNumber <= 1) return;
        CurrentHoleNumber--;
        _currentHoleGps = _holes.FirstOrDefault(h => h.HoleNumber == CurrentHoleNumber);
        UpdateHoleDisplay();
    }

    [RelayCommand]
    private void NextHole()
    {
        if (CurrentHoleNumber >= 18) return;
        CurrentHoleNumber++;
        _currentHoleGps = _holes.FirstOrDefault(h => h.HoleNumber == CurrentHoleNumber);
        UpdateHoleDisplay();
    }

    [RelayCommand]
    private void IncrementScore(PlayerScore player) => player.Score++;

    [RelayCommand]
    private void DecrementScore(PlayerScore player)
    {
        if (player.Score > 1) player.Score--;
    }

    [RelayCommand]
    private async Task SaveScoresAsync()
    {
        // Implemented in Phase 5
        await Task.CompletedTask;
    }
}
