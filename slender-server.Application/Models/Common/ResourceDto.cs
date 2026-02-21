namespace slender_server.Application.Models.Common;

/// <summary>
/// Base class for DTOs that support HATEOAS links
/// </summary>
public abstract class ResourceDto
{
    public List<LinkDto> Links { get; set; } = [];
}