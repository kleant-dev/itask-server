// Domain/Interfaces/IProjectMemberRepository.cs
using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IProjectMemberRepository : IRepository<ProjectMember>
{
    Task<IEnumerable<ProjectMember>> GetByProjectIdAsync(string projectId, CancellationToken cancellationToken = default);
    Task<ProjectMember?> GetMemberAsync(string projectId, string userId, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(string projectId, string userId, CancellationToken cancellationToken = default);
    Task<int> GetMemberCountAsync(string projectId, CancellationToken cancellationToken = default);
}