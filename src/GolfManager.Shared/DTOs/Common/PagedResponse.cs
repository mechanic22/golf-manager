namespace GolfManager.Shared.DTOs.Common;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    public static PagedResponse<T> From(List<T> items, int page, int pageSize, int totalCount) =>
        new() { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
}
