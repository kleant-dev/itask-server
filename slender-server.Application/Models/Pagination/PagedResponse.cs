namespace slender_server.Application.Models.Pagination;

public sealed class PagedResponse<T>
{
    public required List<T> Items { get; set; }
    public required int PageNumber { get; set; }
    public required int PageSize { get; set; }
    public required int TotalCount { get; set; }
    public required int TotalPages { get; set; }
    public required bool HasPrevious { get; set; }
    public required bool HasNext { get; set; }
    
    public static PagedResponse<T> Create(
        List<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        return new PagedResponse<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPrevious = pageNumber > 1,
            HasNext = pageNumber < totalPages
        };
    }
}