namespace BuildingBlocks.Application.Common.Pagination;

public class PagedRequest
{
    public int PageNumber { get; }

    public int PageSize { get; }

    public string? SortBy { get; }

    public bool SortDescending { get; }

    public PagedRequest(int pageNumber = 1, int pageSize = 10, string? sortBy = null, bool sortDescending = false)
    {
        PageNumber = Math.Max(1, pageNumber);
        PageSize = Math.Max(1, Math.Min(100, pageSize)); // Limit page size to 100
        SortBy = sortBy;
        SortDescending = sortDescending;
    }
}
