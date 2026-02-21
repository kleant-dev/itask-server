using slender_server.Application.DTOs.CalendarEventDTOs;
using slender_server.Application.Models.Sorting;

namespace slender_server.Application.SortMappings;

public sealed class CalendarEventSortMapping : SortMappingDefinition<CalendarEventDto, Domain.Entities.CalendarEvent>
{
    public override SortMapping[] GetMappings()
    {
        return
        [
            new SortMapping("id", nameof(Domain.Entities.CalendarEvent.Id)),
            new SortMapping("title", nameof(Domain.Entities.CalendarEvent.Title)),
            new SortMapping("startsAt", nameof(Domain.Entities.CalendarEvent.StartsAtUtc)),
            new SortMapping("endsAt", nameof(Domain.Entities.CalendarEvent.EndsAtUtc)),
            new SortMapping("createdAt", nameof(Domain.Entities.CalendarEvent.CreatedAtUtc))
        ];
    }
}