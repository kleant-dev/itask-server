using slender_server.Application.Models.Sorting;

namespace slender_server.Application.Interfaces.Services;

public interface ISortingService
{
    /// <summary>
    /// Apply sorting to a queryable based on sort string and mappings
    /// </summary>
    IQueryable<TEntity> ApplySort<TDto, TEntity>(
        IQueryable<TEntity> query,
        string? sort,
        string defaultOrderBy = "Id") where TEntity : class;
    
    /// <summary>
    /// Validate that all sort fields are valid for the given DTO/Entity pair
    /// </summary>
    bool ValidateSort<TDto, TEntity>(string? sort) where TEntity : class;
}