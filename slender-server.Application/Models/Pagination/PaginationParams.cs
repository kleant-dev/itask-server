namespace slender_server.Application.Models.Pagination;

public sealed class PaginationParams
{
    private const int MaxPageSize = 100;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get;
        set => field = value > MaxPageSize ? MaxPageSize : value;
    } = 20;
}