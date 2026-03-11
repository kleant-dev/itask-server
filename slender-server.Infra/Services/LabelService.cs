using slender_server.Application.DTOs.LabelDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;

namespace slender_server.Infra.Services;

public sealed class LabelService(
    ILabelRepository labelRepository,
    IWorkspaceMemberRepository workspaceMemberRepository,
    IUnitOfWork unitOfWork)
    : ILabelService
{
    public async Task<Result<LabelDto>> CreateLabelAsync(
        string userId,
        CreateLabelDto dto,
        CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(dto.WorkspaceId, userId, ct);
        if (!isMember)
            return Result<LabelDto>.Failure("You do not have permission to create labels in this workspace");

        var exists = await labelRepository.ExistsAsync(dto.WorkspaceId, dto.Name, ct);
        if (exists)
            return Result<LabelDto>.Failure("A label with this name already exists in this workspace");

        var entity = dto.ToEntity();
        await labelRepository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<LabelDto>.Success(entity.ToDto());
    }

    public async Task<Result<IReadOnlyCollection<LabelDto>>> GetWorkspaceLabelsAsync(
        string workspaceId,
        string userId,
        CancellationToken ct = default)
    {
        var isMember = await workspaceMemberRepository.IsMemberAsync(workspaceId, userId, ct);
        if (!isMember)
            return Result<IReadOnlyCollection<LabelDto>>.Failure("You do not have access to this workspace");

        var labels = await labelRepository.GetByWorkspaceIdAsync(workspaceId, ct);
        var dtos = labels.Select(l => l.ToDto()).ToArray();

        return Result<IReadOnlyCollection<LabelDto>>.Success(dtos);
    }

    public async Task<Result<LabelDto>> UpdateLabelAsync(
        string labelId,
        string userId,
        UpdateLabelDto dto,
        CancellationToken ct = default)
    {
        var label = await labelRepository.GetByIdAsync(labelId, ct);
        if (label is null)
            return Result<LabelDto>.Failure("Label not found");

        var isMember = await workspaceMemberRepository.IsMemberAsync(label.WorkspaceId, userId, ct);
        if (!isMember)
            return Result<LabelDto>.Failure("You do not have permission to update this label");

        dto.ApplyTo(label);
        await labelRepository.UpdateAsync(label, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<LabelDto>.Success(label.ToDto());
    }

    public async Task<Result> DeleteLabelAsync(
        string labelId,
        string userId,
        CancellationToken ct = default)
    {
        var label = await labelRepository.GetByIdAsync(labelId, ct);
        if (label is null)
            return Result.Failure("Label not found");

        var isMember = await workspaceMemberRepository.IsMemberAsync(label.WorkspaceId, userId, ct);
        if (!isMember)
            return Result.Failure("You do not have permission to delete this label");

        var usageCount = await labelRepository.GetUsageCountAsync(label.Id, ct);
        if (usageCount > 0)
            return Result.Failure("Cannot delete a label that is in use");

        await labelRepository.DeleteAsync(label, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}

