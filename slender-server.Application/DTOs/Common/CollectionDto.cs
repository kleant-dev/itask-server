namespace slender_server.Application.DTOs.Common;

/// <summary>
/// Generic wrapper for GET collection responses. Use for list endpoints.
/// </summary>
/// <typeparam name="T">Item DTO type (e.g. UserDto, WorkspaceDto).</typeparam>
public sealed record CollectionDto<T>
{
    public required IReadOnlyList<T> Items { get; init; }
}
