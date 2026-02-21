namespace slender_server.Application.DTOs.TaskCommentDTOs;

public sealed record UpdateTaskCommentDto
{
    public string? Body { get; init; }
}

public static class UpdateTaskCommentDtoExtensions
{
    public static void ApplyTo(this UpdateTaskCommentDto dto, Domain.Entities.TaskComment entity)
    {
        if (dto.Body is not null) entity.Body = dto.Body;
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }
}
