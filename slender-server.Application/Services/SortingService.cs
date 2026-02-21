using System.Linq.Dynamic.Core;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Sorting;

namespace slender_server.Application.Services;

public sealed class SortingService : ISortingService
{
    private readonly IServiceProvider _serviceProvider;

    public SortingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IQueryable<TEntity> ApplySort<TDto, TEntity>(
        IQueryable<TEntity> query,
        string? sort,
        string defaultOrderBy = "Id") where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(defaultOrderBy);
        }

        var mappingDefinition = GetMappingDefinition<TDto, TEntity>();
        var mappings = mappingDefinition.GetMappings();

        string[] sortFields = sort.Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        var orderByParts = new List<string>();
        
        foreach (string field in sortFields)
        {
            (string sortField, bool isDescending) = ParseSortField(field);

            SortMapping? mapping = mappings.FirstOrDefault(m =>
                m.SortField.Equals(sortField, StringComparison.OrdinalIgnoreCase));

            if (mapping is null)
            {
                continue; // Skip invalid fields
            }

            string direction = (isDescending, mapping.Reverse) switch
            {
                (false, false) => "ASC",
                (false, true) => "DESC",
                (true, false) => "DESC",
                (true, true) => "ASC"
            };

            orderByParts.Add($"{mapping.PropertyName} {direction}");
        }

        if (orderByParts.Count == 0)
        {
            return query.OrderBy(defaultOrderBy);
        }

        string orderBy = string.Join(", ", orderByParts);

        return query.OrderBy(orderBy);
    }

    public bool ValidateSort<TDto, TEntity>(string? sort) where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return true;
        }

        var mappingDefinition = GetMappingDefinition<TDto, TEntity>();
        var mappings = mappingDefinition.GetMappings();

        var sortFields = sort
            .Split(',')
            .Select(f => f.Trim().Split(' ')[0])
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .ToList();

        return sortFields.All(f => mappings.Any(m => 
            m.SortField.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }

    private SortMappingDefinition<TDto, TEntity> GetMappingDefinition<TDto, TEntity>() 
        where TEntity : class
    {
        var mappingType = typeof(SortMappingDefinition<TDto, TEntity>);
        
        var mapping = _serviceProvider.GetService(mappingType) as SortMappingDefinition<TDto, TEntity>;
        
        if (mapping is null)
        {
            throw new InvalidOperationException(
                $"Sort mapping for {typeof(TDto).Name} -> {typeof(TEntity).Name} is not registered. " +
                $"Please register it in DependencyInjection.");
        }

        return mapping;
    }

    private static (string SortField, bool IsDescending) ParseSortField(string field)
    {
        string[] parts = field.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string sortField = parts[0];
        bool isDescending = parts.Length > 1 &&
                            parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (sortField, isDescending);
    }
}