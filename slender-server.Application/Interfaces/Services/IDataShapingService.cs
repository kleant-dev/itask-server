using System.Dynamic;
using slender_server.Application.Models.Common;

namespace slender_server.Application.Interfaces.Services;

public interface IDataShapingService
{
    /// <summary>
    /// Shape a single entity to include only requested fields
    /// </summary>
    ExpandoObject ShapeData<T>(T entity, string? fields);

    /// <summary>
    /// Shape a collection of entities to include only requested fields
    /// </summary>
    List<ExpandoObject> ShapeData<T>(
        IEnumerable<T> entities,
        string? fields,
        Func<T, List<LinkDto>>? linksFactory = null);

    /// <summary>
    /// Validate that all requested fields exist on the type
    /// </summary>
    bool ValidateFields<T>(string? fields);

    /// <summary>
    /// Get all available field names for a type
    /// </summary>
    List<string> GetAvailableFields<T>();
}