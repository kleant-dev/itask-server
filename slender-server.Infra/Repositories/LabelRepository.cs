// Infra/Repositories/LabelRepository.cs
using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class LabelRepository(ApplicationDbContext context) : Repository<Label>(context), ILabelRepository
{
    public async Task<IEnumerable<Label>> GetByWorkspaceIdAsync(string workspaceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(l => l.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Label?> GetByNameAsync(string workspaceId, string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(l => l.WorkspaceId == workspaceId && l.Name == name, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string workspaceId, string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(l => l.WorkspaceId == workspaceId && l.Name == name, cancellationToken);
    }

    public async Task<int> GetUsageCountAsync(string labelId, CancellationToken cancellationToken = default)
    {
        return await context.TaskLabels
            .CountAsync(tl => tl.LabelId == labelId, cancellationToken);
    }
}