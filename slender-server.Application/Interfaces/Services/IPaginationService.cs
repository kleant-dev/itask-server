using slender_server.Application.Models.Pagination;
using slender_server.Domain.Models;

namespace slender_server.Application.Interfaces.Services;

public interface IPaginationService
{
    /// <summary>
    /// Map PagedResult from repository to PagedResponse for API
    /// </summary>
    PagedResponse<TDto> MapToPagedResponse<TEntity, TDto>(
        PagedResult<TEntity> pagedResult,
        Func<TEntity, TDto> mapper) where TEntity : class;
}