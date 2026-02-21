using FluentValidation;

namespace slender_server.Application.DTOs.ChannelMemberDTOs.Validators;

public sealed class UpdateChannelMemberDtoValidator : AbstractValidator<UpdateChannelMemberDto>
{
    public UpdateChannelMemberDtoValidator()
    {
        // LastReadAtUtc can be any DateTime value, no specific validation needed
    }
}
