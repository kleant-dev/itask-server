using slender_server.Application.DTOs.WorkspaceDTOs;
using slender_server.Application.Models.Sorting;

namespace slender_server.Application.SortMappings;

public sealed class WorkspaceSortMapping : SortMappingDefinition<WorkspaceDto, Domain.Entities.Workspace>
{
    public override SortMapping[] GetMappings()
    {
        return
        [
            new SortMapping("id", nameof(Domain.Entities.Workspace.Id)),
            new SortMapping("name", nameof(Domain.Entities.Workspace.Name)),
            new SortMapping("slug", nameof(Domain.Entities.Workspace.Slug)),
            new SortMapping("createdAt", nameof(Domain.Entities.Workspace.CreatedAtUtc)),
            new SortMapping("updatedAt", nameof(Domain.Entities.Workspace.UpdatedAtUtc))
        ];
    }
}