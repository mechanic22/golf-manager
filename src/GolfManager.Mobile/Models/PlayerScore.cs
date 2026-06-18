using CommunityToolkit.Mvvm.ComponentModel;

namespace GolfManager.Mobile.Models;

public partial class PlayerScore : ObservableObject
{
    public string LeagueGolferId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string? RoundId { get; set; }

    [ObservableProperty]
    private int _score = 4;
}
