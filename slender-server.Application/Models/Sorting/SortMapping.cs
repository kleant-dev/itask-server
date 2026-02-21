namespace slender_server.Application.Models.Sorting;

/// <summary>
/// Maps a sort field from the DTO to the entity property
/// </summary>
/// <param name="SortField">Field name in the API/DTO (e.g., "createdAt")</param>
/// <param name="PropertyName">Property name in the entity (e.g., "CreatedAtUtc")</param>
/// <param name="Reverse">If true, reverses ASC/DESC direction</param>
public sealed record SortMapping(string SortField, string PropertyName, bool Reverse = false);