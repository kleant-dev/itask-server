using FluentValidation;

namespace slender_server.Application.DTOs.CalendarEventDTOs.Validators;

public sealed class UpdateCalendarEventDtoValidator : AbstractValidator<UpdateCalendarEventDto>
{
    public UpdateCalendarEventDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(255)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .When(x => x.Title is not null)
            .WithMessage("Title cannot be empty or whitespace");

        RuleFor(x => x.TaskId)
            .MaximumLength(50)
            .When(x => x.TaskId is not null);

        RuleFor(x => x.ScheduleType)
            .Must(st => new[] { "Meeting", "Task", "Review", "Event", "Reminder" }.Contains(st))
            .When(x => x.ScheduleType is not null)
            .WithMessage("ScheduleType must be one of: Meeting, Task, Review, Event, Reminder");

        RuleFor(x => x.EndsAtUtc)
            .GreaterThan(x => x.StartsAtUtc)
            .When(x => x.StartsAtUtc is not null && x.EndsAtUtc is not null)
            .WithMessage("EndsAtUtc must be after StartsAtUtc");

        RuleFor(x => x)
            .Must(ce => !ce.IsAllDay.HasValue || !ce.IsAllDay.Value ||
                (ce.StartsAtUtc.HasValue && ce.EndsAtUtc.HasValue &&
                 ce.StartsAtUtc.Value.Hour == 0 && ce.StartsAtUtc.Value.Minute == 0 && ce.StartsAtUtc.Value.Second == 0 &&
                 ce.EndsAtUtc.Value.Hour == 0 && ce.EndsAtUtc.Value.Minute == 0 && ce.EndsAtUtc.Value.Second == 0))
            .When(x => x.IsAllDay is not null && (x.StartsAtUtc is not null || x.EndsAtUtc is not null))
            .WithMessage("All-day events must start and end at midnight (00:00:00)");
    }
}
