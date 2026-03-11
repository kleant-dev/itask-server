using slender_server.Application.DTOs.CalendarEventDTOs;
using slender_server.Application.Models.Common;

namespace slender_server.Application.Interfaces.Services;

public interface ICalendarEventService
{
    Task<Result<CalendarEventDto>> CreateAsync(string userId, CreateCalendarEventDto dto, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CalendarEventDto>>> GetByWorkspaceAsync(string workspaceId, string userId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CalendarEventDto>>> GetByDateRangeAsync(string workspaceId, string userId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<Result<CalendarEventDto>> GetByIdAsync(string eventId, string userId, CancellationToken ct = default);
    Task<Result<CalendarEventDto>> UpdateAsync(string eventId, string userId, UpdateCalendarEventDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(string eventId, string userId, CancellationToken ct = default);
}
