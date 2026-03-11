using slender_server.Application.DTOs.ActivityLog;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;

namespace slender_server.Infra.Services;

public sealed class ActivityLogService(
    IRepository<ActivityLog> activityLogRepository,
    IWorkspaceMemberRepository workspaceMemberRepository,
    IUnitOfWork unitOfWork)
    : IActivityLogService
{
    public async Task<Result<ActivityLogDto>> CreateAsync(
        CreateActivityLogDto dto,
        CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(dto.WorkspaceId, dto.ActorId, ct);
        if (!isMember)
            return Result<ActivityLogDto>.Failure("You do not have access to this workspace");

        var entity = dto.ToEntity();
        await activityLogRepository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<ActivityLogDto>.Success(entity.ToDto());
    }

    public async Task<Result<IReadOnlyCollection<ActivityLogDto>>> GetWorkspaceLogsAsync(
        string workspaceId,
        string userId,
        CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, userId, ct);
        if (!isMember)
            return Result<IReadOnlyCollection<ActivityLogDto>>.Failure("You do not have access to this workspace");

        var all = await activityLogRepository.GetAllAsync(ct);
        var logs = all
            .Where(l => l.WorkspaceId == workspaceId)
            .OrderByDescending(l => l.CreatedAtUtc)
            .Select(l => l.ToDto())
            .ToArray();

        return Result<IReadOnlyCollection<ActivityLogDto>>.Success(logs);
    }
}

