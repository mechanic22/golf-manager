using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GolfManager.Mobile.Services;
using GolfManager.Shared.DTOs.League;
using System.Collections.ObjectModel;

namespace GolfManager.Mobile.ViewModels;

public partial class LeagueSelectViewModel : ObservableObject
{
    private readonly ILeagueService _leagueService;
    private readonly IAuthService _auth;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<LeagueResponse> Leagues { get; } = new();

    public LeagueSelectViewModel(ILeagueService leagueService, IAuthService auth)
    {
        _leagueService = leagueService;
        _auth = auth;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var leagues = await _leagueService.GetLeaguesAsync();
            Leagues.Clear();
            foreach (var l in leagues ?? []) Leagues.Add(l);
        }
        catch { ErrorMessage = "Failed to load leagues."; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }

    [RelayCommand]
    private async Task SelectLeagueAsync(LeagueResponse league)
    {
        if (string.IsNullOrEmpty(league.ActiveSeasonId))
        {
            ErrorMessage = "This league has no active season.";
            return;
        }
        await Shell.Current.GoToAsync("week-select", new Dictionary<string, object>
        {
            ["league"] = league
        });
    }
}
