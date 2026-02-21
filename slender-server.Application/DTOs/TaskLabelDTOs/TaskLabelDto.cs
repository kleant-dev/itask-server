namespace slender_server.Application.DTOs.TaskLabelDTOs;
using Domain.Entities;

public sealed record TaskLabelDto
{
    public required string TaskId { get; init; }
    public required string LabelId { get; init; }
}

public static class TaskLabelDtoExtensions
{
    public static TaskLabelDto ToDto(this TaskLabel entity)
    {
        return new TaskLabelDto
        {
            TaskId = entity.TaskId,
            LabelId = entity.LabelId
        };
    }
}
