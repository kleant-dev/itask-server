using slender_server.Application.DTOs.TaskDTOs;
using slender_server.Application.Models.Sorting;

namespace slender_server.Application.SortMappings;

public sealed class TaskSortMapping : SortMappingDefinition<TaskDto, Domain.Entities.Task>
{
    public override SortMapping[] GetMappings()
    {
        return
        [
            new SortMapping("id", nameof(Domain.Entities.Task.Id)),
            new SortMapping("title", nameof(Domain.Entities.Task.Title)),
            new SortMapping("status", nameof(Domain.Entities.Task.Status)),
            new SortMapping("priority", nameof(Domain.Entities.Task.Priority)),
            new SortMapping("dueDate", nameof(Domain.Entities.Task.DueDate)),
            new SortMapping("scheduledAt", nameof(Domain.Entities.Task.ScheduledAt)),
            new SortMapping("createdAt", nameof(Domain.Entities.Task.CreatedAtUtc)),
            new SortMapping("updatedAt", nameof(Domain.Entities.Task.UpdatedAtUtc)),
            new SortMapping("sortOrder", nameof(Domain.Entities.Task.SortOrder))
        ];
    }
}