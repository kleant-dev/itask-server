// Domain/Interfaces/ILabelRepository.cs
using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface ILabelRepository : IRepository<Label>
{
    Task<IEnumerable<Label>> GetByWorkspaceIdAsync(string workspaceId, CancellationToken cancellationToken = default);
    Task<Label?> GetByNameAsync(string workspaceId, string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string workspaceId, string name, CancellationToken cancellationToken = default);
    Task<int> GetUsageCountAsync(string labelId, CancellationToken cancellationToken = default);
}