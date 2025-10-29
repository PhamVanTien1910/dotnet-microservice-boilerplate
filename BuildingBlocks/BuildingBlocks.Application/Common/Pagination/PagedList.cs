namespace BuildingBlocks.Application.Common.Pagination;

public class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber * PageSize < TotalCount;

    public PagedList(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}