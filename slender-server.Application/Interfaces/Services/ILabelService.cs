using slender_server.Application.DTOs.LabelDTOs;
using slender_server.Application.Models.Common;

namespace slender_server.Application.Interfaces.Services;

public interface ILabelService
{
    Task<Result<LabelDto>> CreateLabelAsync(
        string userId,
        CreateLabelDto dto,
        CancellationToken ct = default);

    Task<Result<IReadOnlyCollection<LabelDto>>> GetWorkspaceLabelsAsync(
        string workspaceId,
        string userId,
        CancellationToken ct = default);

    Task<Result<LabelDto>> UpdateLabelAsync(
        string labelId,
        string userId,
        UpdateLabelDto dto,
        CancellationToken ct = default);

    Task<Result> DeleteLabelAsync(
        string labelId,
        string userId,
        CancellationToken ct = default);
}

