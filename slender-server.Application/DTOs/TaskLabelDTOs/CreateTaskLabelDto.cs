namespace slender_server.Application.DTOs.TaskLabelDTOs;
using Domain.Entities;

public sealed record CreateTaskLabelDto
{
    public required string TaskId { get; init; }
    public required string LabelId { get; init; }
}

public static class CreateTaskLabelDtoExtensions
{
    public static TaskLabel ToEntity(this CreateTaskLabelDto dto)
    {
        return new TaskLabel()
        {
            TaskId = dto.TaskId,
            LabelId = dto.LabelId
        };
    }
}
