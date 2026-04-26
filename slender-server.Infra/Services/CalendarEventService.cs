using slender_server.Application.DTOs.CalendarEventDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Domain.Interfaces;

namespace slender_server.Infra.Services;

public sealed class CalendarEventService(
    ICalendarEventRepository calendarEventRepository,
    IWorkspaceMemberRepository workspaceMemberRepository,
    IUnitOfWork unitOfWork)
    : ICalendarEventService
{
    public async Task<Result<CalendarEventDto>> CreateAsync(string userId, CreateCalendarEventDto dto, CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(dto.WorkspaceId, userId, ct);
        if (!isMember)
            return Result<CalendarEventDto>.Failure("You do not have access to this workspace",ErrorType.Forbidden);

        var entity = (dto with { CreatedById = userId }).ToEntity();
        await calendarEventRepository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<CalendarEventDto>.Success(entity.ToDto());
    }

    public async Task<Result<IReadOnlyList<CalendarEventDto>>> GetByWorkspaceAsync(string workspaceId, string userId, CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, userId, ct);
        if (!isMember)
            return Result<IReadOnlyList<CalendarEventDto>>.Failure("You do not have access to this workspace",ErrorType.Forbidden);

        var list = await calendarEventRepository.GetByWorkspaceIdAsync(workspaceId, ct);
        return Result<IReadOnlyList<CalendarEventDto>>.Success(list.Select(e => e.ToDto()).ToArray());
    }

    public async Task<Result<IReadOnlyList<CalendarEventDto>>> GetByDateRangeAsync(string workspaceId, string userId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, userId, ct);
        if (!isMember)
            return Result<IReadOnlyList<CalendarEventDto>>.Failure("You do not have access to this workspace",ErrorType.Forbidden);

        var list = await calendarEventRepository.GetByWorkspaceAndDateRangeAsync(workspaceId, fromUtc, toUtc, ct);
        return Result<IReadOnlyList<CalendarEventDto>>.Success(list.Select(e => e.ToDto()).ToArray());
    }

    public async Task<Result<CalendarEventDto>> GetByIdAsync(string eventId, string userId, CancellationToken ct = default)
    {
        var entity = await calendarEventRepository.GetByIdAsync(eventId, ct);
        if (entity is null)
            return Result<CalendarEventDto>.Failure("Event not found",ErrorType.NotFound);

        var isMember = await workspaceMemberRepository.IsMemberAsync(entity.WorkspaceId, userId, ct);
        if (!isMember)
            return Result<CalendarEventDto>.Failure("You do not have access to this event",ErrorType.Forbidden);

        return Result<CalendarEventDto>.Success(entity.ToDto());
    }

    public async Task<Result<CalendarEventDto>> UpdateAsync(string eventId, string userId, UpdateCalendarEventDto dto, CancellationToken ct = default)
    {
        var entity = await calendarEventRepository.GetByIdAsync(eventId, ct);
        if (entity is null)
            return Result<CalendarEventDto>.Failure("Event not found",ErrorType.NotFound);

        var isMember = await workspaceMemberRepository.IsMemberAsync(entity.WorkspaceId, userId, ct);
        if (!isMember)
            return Result<CalendarEventDto>.Failure("You do not have access to this event",ErrorType.Forbidden);
        if (entity.CreatedById != userId)
            return Result<CalendarEventDto>.Failure("Only the creator can update this event",ErrorType.Forbidden);

        dto.ApplyTo(entity);
        await calendarEventRepository.UpdateAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<CalendarEventDto>.Success(entity.ToDto());
    }

    public async Task<Result> DeleteAsync(string eventId, string userId, CancellationToken ct = default)
    {
        var entity = await calendarEventRepository.GetByIdAsync(eventId, ct);
        if (entity is null)
            return Result.Failure("Event not found",ErrorType.NotFound);

        var isMember = await workspaceMemberRepository.IsMemberAsync(entity.WorkspaceId, userId, ct);
        if (!isMember)
            return Result.Failure("You do not have access to this event",ErrorType.Forbidden);
        if (entity.CreatedById != userId)
            return Result.Failure("Only the creator can delete this event",ErrorType.Forbidden);

        await calendarEventRepository.DeleteAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
