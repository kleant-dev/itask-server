namespace slender_server.Application.Models.Sorting;

/// <summary>
/// Base class for defining sort mappings between DTOs and Entities
/// </summary>
public abstract class SortMappingDefinition<TDto, TEntity> where TEntity : class
{
    public abstract SortMapping[] GetMappings();
}