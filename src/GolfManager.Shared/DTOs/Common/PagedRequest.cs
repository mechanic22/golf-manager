namespace GolfManager.Shared.DTOs.Common;

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    public int SafePageSize => Math.Clamp(PageSize, 1, 100);
    public int SafePage => Math.Max(Page, 1);
}
