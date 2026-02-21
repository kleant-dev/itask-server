using slender_server.Application.DTOs.ProjectDTOs;
using slender_server.Application.Models.Sorting;

namespace slender_server.Application.SortMappings;

public sealed class ProjectSortMapping : SortMappingDefinition<ProjectDto, Domain.Entities.Project>
{
    public override SortMapping[] GetMappings()
    {
        return
        [
            new SortMapping("id", nameof(Domain.Entities.Project.Id)),
            new SortMapping("name", nameof(Domain.Entities.Project.Name)),
            new SortMapping("status", nameof(Domain.Entities.Project.Status)),
            new SortMapping("createdAt", nameof(Domain.Entities.Project.CreatedAtUtc)),
            new SortMapping("updatedAt", nameof(Domain.Entities.Project.UpdatedAtUtc)),
            new SortMapping("startDate", nameof(Domain.Entities.Project.StartDate)),
            new SortMapping("targetDate", nameof(Domain.Entities.Project.TargetDate))
        ];
    }
}