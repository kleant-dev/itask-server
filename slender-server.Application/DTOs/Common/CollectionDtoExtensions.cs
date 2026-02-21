namespace slender_server.Application.DTOs.Common;

public static class CollectionDtoExtensions
{
    /// <summary>
    /// Maps a sequence of entities to a collection DTO using the provided mapper (e.g. entity => entity.ToDto()).
    /// </summary>
    public static CollectionDto<TDto> ToCollectionDto<TEntity, TDto>(
        this IEnumerable<TEntity> source,
        Func<TEntity, TDto> map)
    {
        var items = source.Select(map).ToList().AsReadOnly();
        return new CollectionDto<TDto> { Items = items };
    }
}
