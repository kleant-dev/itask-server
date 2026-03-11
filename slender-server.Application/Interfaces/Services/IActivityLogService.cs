using slender_server.Application.DTOs.ActivityLog;
using slender_server.Application.Models.Common;

namespace slender_server.Application.Interfaces.Services;

public interface IActivityLogService
{
    Task<Result<ActivityLogDto>> CreateAsync(
        CreateActivityLogDto dto,
        CancellationToken ct = default);

    Task<Result<IReadOnlyCollection<ActivityLogDto>>> GetWorkspaceLogsAsync(
        string workspaceId,
        string userId,
        CancellationToken ct = default);
}

