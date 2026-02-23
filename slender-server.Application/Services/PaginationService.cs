using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Pagination;
using slender_server.Domain.Models;

namespace slender_server.Application.Services;

public sealed class PaginationService : IPaginationService
{
    public PagedResponse<TDto> MapToPagedResponse<TEntity, TDto>(
        PagedResult<TEntity> pagedResult,
        Func<TEntity, TDto> mapper) where TEntity : class
    {
        List<TDto> mappedItems = pagedResult.Items.Select(mapper).ToList();
        
        return PagedResponse<TDto>.Create(
            mappedItems,
            pagedResult.PageNumber,
            pagedResult.PageSize,
            pagedResult.TotalCount);
    }
}