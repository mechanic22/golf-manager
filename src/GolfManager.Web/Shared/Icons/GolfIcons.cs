namespace GolfManager.Web.Shared.Icons;

/// <summary>
/// Golf-themed and common UI icon SVG paths
/// All icons are designed for a 24x24 viewBox
/// </summary>
public static class GolfIcons
{
    // ============================================================================
    // GOLF-SPECIFIC ICONS
    // ============================================================================
    
    /// <summary>Golf flag icon</summary>
    public const string GolfFlag = "M6 3v18h2V3H6zm2 2h10l-5 5 5 5H8V5z";
    
    /// <summary>Trophy/award icon</summary>
    public const string Trophy = "M19 5h-2V3H7v2H5c-1.1 0-2 .9-2 2v1c0 2.55 1.92 4.63 4.39 4.94.63 1.5 1.98 2.63 3.61 2.96V19H7v2h10v-2h-4v-3.1c1.63-.33 2.98-1.46 3.61-2.96C19.08 12.63 21 10.55 21 8V7c0-1.1-.9-2-2-2zM5 8V7h2v3.82C5.84 10.4 5 9.3 5 8zm14 0c0 1.3-.84 2.4-2 2.82V7h2v1z";
    
    /// <summary>Scorecard icon</summary>
    public const string Scorecard = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 14H7v-2h5v2zm3-4H7v-2h8v2zm0-4H7V7h8v2z";
    
    /// <summary>Leaderboard icon</summary>
    public const string Leaderboard = "M7.5 21H2V9h5.5zm7.25-18h-5.5v18h5.5zM22 11h-5.5v10H22z";
    
    /// <summary>Golf course/terrain icon</summary>
    public const string GolfCourse = "M19.5 19.5c-.8 0-1.5.7-1.5 1.5s.7 1.5 1.5 1.5 1.5-.7 1.5-1.5-.7-1.5-1.5-1.5M17 5.92L9 2v18H7v-1.73c-1.79.35-3 .99-3 1.73 0 1.1 2.69 2 6 2s6-.9 6-2c0-.99-2.16-1.81-5-1.97V8.98l6-3.06z";
    
    /// <summary>Target/hole icon</summary>
    public const string Target = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm0-14c-3.31 0-6 2.69-6 6s2.69 6 6 6 6-2.69 6-6-2.69-6-6-6zm0 10c-2.21 0-4-1.79-4-4s1.79-4 4-4 4 1.79 4 4-1.79 4-4 4zm0-6c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2z";
    
    // ============================================================================
    // COMMON UI ICONS
    // ============================================================================
    
    /// <summary>Home icon</summary>
    public const string Home = "M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z";
    
    /// <summary>Dashboard/grid icon</summary>
    public const string Dashboard = "M3 13h8V3H3v10zm0 8h8v-6H3v6zm10 0h8V11h-8v10zm0-18v6h8V3h-8z";
    
    /// <summary>People/users icon</summary>
    public const string People = "M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5c-1.66 0-3 1.34-3 3s1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5C6.34 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z";
    
    /// <summary>Users icon alias</summary>
    public const string Users = People;

    /// <summary>Layers/stack icon</summary>
    public const string Layers = "M12 2L2 7l10 5 10-5-10-5zm0 5.5L4.5 7 12 11.5 19.5 7 12 7.5zm0 5.5L4.5 12 12 16.5 19.5 12 12 13z";

    /// <summary>Building icon</summary>
    public const string Building = "M4 22h16V8l-8-5-8 5v14zm2-2v-8h4v8H6zm6 0v-4h4v4h-4zm-6-6V6.5L12 3l6 3.5V14H6z";

    /// <summary>Bar chart icon</summary>
    public const string BarChart = "M3 17h4V9H3v8zm6 0h4V5h-4v12zm6 0h4V13h-4v4z";

    /// <summary>Person/user icon</summary>
    public const string Person = "M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z";
    
    /// <summary>Calendar/date icon</summary>
    public const string Calendar = "M19 3h-1V1h-2v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H5V8h14v11zM7 10h5v5H7z";
    
    /// <summary>Event/schedule icon</summary>
    public const string Event = "M17 12h-5v5h5v-5zM16 1v2H8V1H6v2H5c-1.11 0-1.99.9-1.99 2L3 19c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2h-1V1h-2zm3 18H5V8h14v11z";
    
    /// <summary>Settings/gear icon</summary>
    public const string Settings = "M19.14 12.94c.04-.3.06-.61.06-.94 0-.32-.02-.64-.07-.94l2.03-1.58c.18-.14.23-.41.12-.61l-1.92-3.32c-.12-.22-.37-.29-.59-.22l-2.39.96c-.5-.38-1.03-.7-1.62-.94L14.4 2.81c-.04-.24-.24-.41-.48-.41h-3.84c-.24 0-.43.17-.47.41l-.36 2.54c-.59.24-1.13.57-1.62.94l-2.39-.96c-.22-.08-.47 0-.59.22L2.74 8.87c-.12.21-.08.47.12.61l2.03 1.58c-.05.3-.09.63-.09.94s.02.64.07.94l-2.03 1.58c-.18.14-.23.41-.12.61l1.92 3.32c.12.22.37.29.59.22l2.39-.96c.5.38 1.03.7 1.62.94l.36 2.54c.05.24.24.41.48.41h3.84c.24 0 .44-.17.47-.41l.36-2.54c.59-.24 1.13-.56 1.62-.94l2.39.96c.22.08.47 0 .59-.22l1.92-3.32c.12-.22.07-.47-.12-.61l-2.01-1.58zM12 15.6c-1.98 0-3.6-1.62-3.6-3.6s1.62-3.6 3.6-3.6 3.6 1.62 3.6 3.6-1.62 3.6-3.6 3.6z";
    
    /// <summary>Add/plus icon</summary>
    public const string Add = "M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z";
    
    /// <summary>Edit/pencil icon</summary>
    public const string Edit = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z";
    
    /// <summary>Delete/trash icon</summary>
    public const string Delete = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z";
    
    /// <summary>Close/X icon</summary>
    public const string Close = "M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z";
    
    /// <summary>Check/checkmark icon</summary>
    public const string Check = "M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z";
    
    /// <summary>Search/magnifying glass icon</summary>
    public const string Search = "M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z";
    
    /// <summary>Menu/hamburger icon</summary>
    public const string Menu = "M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z";
    
    /// <summary>More vertical (three dots)</summary>
    public const string MoreVert = "M12 8c1.1 0 2-.9 2-2s-.9-2-2-2-2 .9-2 2 .9 2 2 2zm0 2c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm0 6c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2z";
    
    /// <summary>Arrow back/left</summary>
    public const string ArrowBack = "M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z";
    
    /// <summary>Arrow forward/right</summary>
    public const string ArrowForward = "M12 4l-1.41 1.41L16.17 11H4v2h12.17l-5.58 5.59L12 20l8-8z";
    
    /// <summary>Logout/sign out icon</summary>
    public const string Logout = "M17 7l-1.41 1.41L18.17 11H8v2h10.17l-2.58 2.58L17 17l5-5zM4 5h8V3H4c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h8v-2H4V5z";
    
    /// <summary>Login/sign in icon</summary>
    public const string Login = "M11 7L9.6 8.4l2.6 2.6H2v2h10.2l-2.6 2.6L11 17l5-5zm9 12h-8v2h8c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2h-8v2h8v14z";
    
    /// <summary>Star icon</summary>
    public const string Star = "M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z";
    
    /// <summary>Info/information icon</summary>
    public const string Info = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z";
    
    /// <summary>Warning/alert icon</summary>
    public const string Warning = "M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z";
    
    /// <summary>Trending up icon</summary>
    public const string TrendingUp = "M16 6l2.29 2.29-4.88 4.88-4-4L2 16.59 3.41 18l6-6 4 4 6.3-6.29L22 12V6z";
    
    /// <summary>Trending down icon</summary>
    public const string TrendingDown = "M16 18l2.29-2.29-4.88-4.88-4 4L2 7.41 3.41 6l6 6 4-4 6.3 6.29L22 12v6z";

    /// <summary>Lock/padlock icon (Heroicons outline)</summary>
    public const string Lock = "M16.5 10.5V6.75a4.5 4.5 0 1 0-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 0 0 2.25-2.25v-6.75a2.25 2.25 0 0 0-2.25-2.25H6.75a2.25 2.25 0 0 0-2.25 2.25v6.75a2.25 2.25 0 0 0 2.25 2.25z";
}

