using slender_server.Application.Models.Pagination;
using slender_server.Domain.Interfaces;

namespace slender_server.Application.Interfaces.Services;

public interface IPaginationService
{
    PagedResponse<TDto> MapToPagedResponse<TEntity, TDto>(
        PagedResult<TEntity> pagedResult,
        Func<TEntity, TDto> mapper) where TEntity : class;
}