using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface ICalendarEventRepository : IRepository<CalendarEvent>
{
    Task<List<CalendarEvent>> GetByWorkspaceIdAsync(string workspaceId, CancellationToken ct = default);
    Task<List<CalendarEvent>> GetByWorkspaceAndDateRangeAsync(string workspaceId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
}
