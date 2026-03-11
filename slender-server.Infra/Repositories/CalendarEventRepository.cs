using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class CalendarEventRepository(ApplicationDbContext context) : Repository<CalendarEvent>(context), ICalendarEventRepository
{
    public async Task<List<CalendarEvent>> GetByWorkspaceIdAsync(string workspaceId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(ce => ce.WorkspaceId == workspaceId)
            .OrderBy(ce => ce.StartsAtUtc)
            .ToListAsync(ct);
    }

    public async Task<List<CalendarEvent>> GetByWorkspaceAndDateRangeAsync(string workspaceId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        return await DbSet
            .Where(ce => ce.WorkspaceId == workspaceId
                && ce.StartsAtUtc < toUtc
                && ce.EndsAtUtc > fromUtc)
            .OrderBy(ce => ce.StartsAtUtc)
            .ToListAsync(ct);
    }
}
