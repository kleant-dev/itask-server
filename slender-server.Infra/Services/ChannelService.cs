using slender_server.Application.DTOs.ChannelDTOs;
using slender_server.Application.DTOs.ChannelMemberDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;

namespace slender_server.Infra.Services;

public sealed class ChannelService(
    IChannelRepository channelRepository,
    IChannelMemberRepository channelMemberRepository,
    IWorkspaceMemberRepository workspaceMemberRepository,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService)
    : IChannelService
{
    public async Task<Result<ChannelDto>> CreateAsync(string userId, CreateChannelDto dto, CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(dto.WorkspaceId, userId, ct);
        if (!isMember)
            return Result<ChannelDto>.Failure("You do not have access to this workspace",ErrorType.Forbidden);

        var entity = (dto with { CreatedById = userId }).ToEntity();
        await channelRepository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<ChannelDto>.Success(entity.ToDto());
    }

    public async Task<Result<ChannelDto>> GetOrCreateDmAsync(string userId, string workspaceId, string otherUserId, CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, userId, ct);
        if (!isMember)
            return Result<ChannelDto>.Failure("You do not have access to this workspace",ErrorType.Forbidden);

        var hash = Channel.GetParticipantHash(userId, otherUserId);
        var channel = await channelRepository.GetByParticipantHashAsync(workspaceId, hash, ct);
        if (channel is not null)
            return Result<ChannelDto>.Success(channel.ToDto());

        channel = Channel.CreateDirectMessage(workspaceId, userId, userId, otherUserId);
        await channelRepository.AddAsync(channel, ct);

        var creatorMember = new ChannelMember { ChannelId = channel.Id, UserId = userId, JoinedAtUtc = DateTime.UtcNow };
        var otherMember = new ChannelMember { ChannelId = channel.Id, UserId = otherUserId, JoinedAtUtc = DateTime.UtcNow };
        await channelMemberRepository.AddAsync(creatorMember, ct);
        await channelMemberRepository.AddAsync(otherMember, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<ChannelDto>.Success(channel.ToDto());
    }

    public async Task<Result<PagedResponse<ChannelDto>>> GetByWorkspaceAsync(string workspaceId, string userId, PaginationParams pagination, CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, userId, ct);
        if (!isMember)
            return Result<PagedResponse<ChannelDto>>.Failure("You do not have access to this workspace",ErrorType.Forbidden);

        var paged = await channelRepository.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            c => c.WorkspaceId == workspaceId,
            q => q.OrderByDescending(c => c.CreatedAtUtc),
            ct);
        var response = paginationService.MapToPagedResponse(paged, c => c.ToDto());
        return Result<PagedResponse<ChannelDto>>.Success(response);
    }

    public async Task<Result<ChannelDto>> GetByIdAsync(string channelId, string userId, CancellationToken ct = default)
    {
        var channel = await channelRepository.GetByIdWithMembersAsync(channelId, ct);
        if (channel is null)
            return Result<ChannelDto>.Failure("Channel not found",ErrorType.NotFound);

        var isMember = await channelMemberRepository.IsMemberAsync(channelId, userId, ct);
        if (!isMember)
            return Result<ChannelDto>.Failure("You do not have access to this channel",ErrorType.Forbidden);
        return Result<ChannelDto>.Success(channel.ToDto());
    }

    public async Task<Result<ChannelDto>> UpdateAsync(string channelId, string userId, UpdateChannelDto dto, CancellationToken ct = default)
    {
        var channel = await channelRepository.GetByIdAsync(channelId, ct);
        if (channel is null)
            return Result<ChannelDto>.Failure("Channel not found",ErrorType.NotFound);

        var isMember = await channelMemberRepository.IsMemberAsync(channelId, userId, ct);
        if (!isMember)
            return Result<ChannelDto>.Failure("You do not have access to this channel",ErrorType.Forbidden);
        dto.ApplyTo(channel);
        await channelRepository.UpdateAsync(channel, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<ChannelDto>.Success(channel.ToDto());
    }

    public async Task<Result> DeleteAsync(string channelId, string userId, CancellationToken ct = default)
    {
        var channel = await channelRepository.GetByIdAsync(channelId, ct);
        if (channel is null)
            return Result.Failure("Channel not found",ErrorType.NotFound);

        var isMember = await channelMemberRepository.IsMemberAsync(channelId, userId, ct);
        if (!isMember)
            return Result.Failure("You do not have access to this channel",ErrorType.Forbidden);
        if (channel.CreatedById != userId)
            return Result.Failure("Only the channel creator can delete it",ErrorType.Forbidden);

        await channelRepository.DeleteAsync(channel, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ChannelMemberDto>>> GetMembersAsync(string channelId, string userId, CancellationToken ct = default)
    {
        var isMember = await channelMemberRepository.IsMemberAsync(channelId, userId, ct);
        if (!isMember)
            return Result<IReadOnlyList<ChannelMemberDto>>.Failure("You do not have access to this channel",ErrorType.Forbidden);
        var members = await channelMemberRepository.GetByChannelIdAsync(channelId, ct);
        return Result<IReadOnlyList<ChannelMemberDto>>.Success(members.Select(m => m.ToDto()).ToArray());
    }

    public async Task<Result<ChannelMemberDto>> JoinAsync(string channelId, string userId, CancellationToken ct = default)
    {
        var channel = await channelRepository.GetByIdAsync(channelId, ct);
        if (channel is null)
            return Result<ChannelMemberDto>.Failure("Channel not found",ErrorType.NotFound);

        var isWorkspaceMember = await workspaceMemberRepository.IsMemberAsync(channel.WorkspaceId, userId, ct);
        if (!isWorkspaceMember)
            return Result<ChannelMemberDto>.Failure("You must be a workspace member to join this channel",ErrorType.Forbidden);

        var existing = await channelMemberRepository.GetByChannelAndUserAsync(channelId, userId, ct);
        if (existing is not null)
            return Result<ChannelMemberDto>.Success(existing.ToDto());

        var member = new ChannelMember { ChannelId = channelId, UserId = userId, JoinedAtUtc = DateTime.UtcNow };
        await channelMemberRepository.AddAsync(member, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<ChannelMemberDto>.Success(member.ToDto());
    }

    public async Task<Result> LeaveAsync(string channelId, string userId, CancellationToken ct = default)
    {
        var member = await channelMemberRepository.GetByChannelAndUserAsync(channelId, userId, ct);
        if (member is null)
            return Result.Failure("You are not a member of this channel",ErrorType.NotFound);

        await channelMemberRepository.RemoveAsync(member, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
