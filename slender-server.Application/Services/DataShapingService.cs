// slender-server.Infra/Services/DataShapingService.cs

using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;

namespace slender_server.Application.Services;

public sealed class DataShapingService : IDataShapingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache = new();

    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        HashSet<string> fieldsSet = ParseFields(fields);
        PropertyInfo[] propertyInfos = GetPropertiesForType<T>();

        if (fieldsSet.Any())
        {
            propertyInfos = propertyInfos
                .Where(p => fieldsSet.Contains(p.Name))
                .ToArray();
        }

        IDictionary<string, object?> shapedObject = new ExpandoObject();

        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            object? value = propertyInfo.GetValue(entity);
            shapedObject[propertyInfo.Name] = value;
        }

        return (ExpandoObject)shapedObject;
    }

    public List<ExpandoObject> ShapeData<T>(
        IEnumerable<T> entities,
        string? fields,
        Func<T, List<LinkDto>>? linksFactory = null)
    {
        if (entities is null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        HashSet<string> fieldsSet = ParseFields(fields);
        PropertyInfo[] propertyInfos = GetPropertiesForType<T>();

        if (fieldsSet.Any())
        {
            propertyInfos = propertyInfos
                .Where(p => fieldsSet.Contains(p.Name))
                .ToArray();
        }

        List<ExpandoObject> shapedObjects = [];

        foreach (T entity in entities)
        {
            IDictionary<string, object?> shapedObject = new ExpandoObject();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                object? value = propertyInfo.GetValue(entity);
                shapedObject[propertyInfo.Name] = value;
            }

            // Add HATEOAS links if factory provided
            if (linksFactory is not null)
            {
                shapedObject["links"] = linksFactory(entity);
            }

            shapedObjects.Add((ExpandoObject)shapedObject);
        }

        return shapedObjects;
    }

    public bool ValidateFields<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }

        HashSet<string> fieldsSet = ParseFields(fields);
        PropertyInfo[] propertyInfos = GetPropertiesForType<T>();

        return fieldsSet.All(field =>
            propertyInfos.Any(p => p.Name.Equals(field, StringComparison.OrdinalIgnoreCase)));
    }

    public List<string> GetAvailableFields<T>()
    {
        PropertyInfo[] propertyInfos = GetPropertiesForType<T>();
        return propertyInfos.Select(p => p.Name).ToList();
    }

    private static HashSet<string> ParseFields(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return [];
        }

        return fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static PropertyInfo[] GetPropertiesForType<T>()
    {
        return PropertiesCache.GetOrAdd(
            typeof(T),
            type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance));
    }
}