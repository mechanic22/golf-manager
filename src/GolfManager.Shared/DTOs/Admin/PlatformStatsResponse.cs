namespace GolfManager.Shared.DTOs.Admin;

/// <summary>
/// Platform-wide statistics for admin dashboard
/// </summary>
public class PlatformStatsResponse
{
    /// <summary>
    /// Total number of users
    /// </summary>
    public int TotalUsers { get; set; }
    
    /// <summary>
    /// Number of active users
    /// </summary>
    public int ActiveUsers { get; set; }
    
    /// <summary>
    /// Number of new users this month
    /// </summary>
    public int NewUsersThisMonth { get; set; }
    
    /// <summary>
    /// Total number of leagues
    /// </summary>
    public int TotalLeagues { get; set; }
    
    /// <summary>
    /// Number of active leagues
    /// </summary>
    public int ActiveLeagues { get; set; }
    
    /// <summary>
    /// Total number of seasons across all leagues
    /// </summary>
    public int TotalSeasons { get; set; }
    
    /// <summary>
    /// Number of active/current seasons
    /// </summary>
    public int ActiveSeasons { get; set; }
    
    /// <summary>
    /// Total number of events across all leagues
    /// </summary>
    public int TotalEvents { get; set; }
    
    /// <summary>
    /// Number of upcoming events
    /// </summary>
    public int UpcomingEvents { get; set; }
    
    /// <summary>
    /// Total number of rounds recorded
    /// </summary>
    public int TotalRounds { get; set; }
    
    /// <summary>
    /// Number of rounds recorded this month
    /// </summary>
    public int RoundsThisMonth { get; set; }
    
    /// <summary>
    /// Number of global admins
    /// </summary>
    public int GlobalAdminCount { get; set; }
}
