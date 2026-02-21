using slender_server.Application.DTOs.NotificationDTOs;
using slender_server.Application.Models.Sorting;

namespace slender_server.Application.SortMappings;

public sealed class NotificationSortMapping : SortMappingDefinition<NotificationDto, Domain.Entities.Notification>
{
    public override SortMapping[] GetMappings()
    {
        return
        [
            new SortMapping("id", nameof(Domain.Entities.Notification.Id)),
            new SortMapping("type", nameof(Domain.Entities.Notification.Type)),
            new SortMapping("createdAt", nameof(Domain.Entities.Notification.CreatedAtUtc)),
            new SortMapping("readAt", nameof(Domain.Entities.Notification.ReadAtUtc)),
            // Sort by read status (unread first)
            new SortMapping("unread", nameof(Domain.Entities.Notification.ReadAtUtc), Reverse: true)
        ];
    }
}