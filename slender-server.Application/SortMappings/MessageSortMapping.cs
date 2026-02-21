// slender-server.Application/SortMappings/MessageSortMapping.cs
using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.Models.Sorting;

namespace slender_server.Application.SortMappings;

public sealed class MessageSortMapping : SortMappingDefinition<MessageDto, Domain.Entities.Message>
{
    public override SortMapping[] GetMappings()
    {
        return
        [
            new SortMapping("id", nameof(Domain.Entities.Message.Id)),
            new SortMapping("createdAt", nameof(Domain.Entities.Message.CreatedAtUtc)),
            new SortMapping("updatedAt", nameof(Domain.Entities.Message.UpdatedAtUtc))
        ];
    }
}