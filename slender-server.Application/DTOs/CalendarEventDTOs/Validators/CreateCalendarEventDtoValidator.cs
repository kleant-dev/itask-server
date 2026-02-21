using FluentValidation;

namespace slender_server.Application.DTOs.CalendarEventDTOs.Validators;

public sealed class CreateCalendarEventDtoValidator : AbstractValidator<CreateCalendarEventDto>
{
    public CreateCalendarEventDtoValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.CreatedById)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.TaskId)
            .MaximumLength(50)
            .When(x => x.TaskId is not null);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(255)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title cannot be empty or whitespace");

        RuleFor(x => x.ScheduleType)
            .Must(st => st == null || new[] { "Meeting", "Task", "Review", "Event", "Reminder" }.Contains(st))
            .When(x => x.ScheduleType is not null)
            .WithMessage("ScheduleType must be one of: Meeting, Task, Review, Event, Reminder");

        RuleFor(x => x.StartsAtUtc)
            .NotEmpty();

        RuleFor(x => x.EndsAtUtc)
            .NotEmpty()
            .GreaterThan(x => x.StartsAtUtc)
            .WithMessage("EndsAtUtc must be after StartsAtUtc");

        RuleFor(x => x)
            .Must(ce => !ce.IsAllDay || 
                (ce.StartsAtUtc.Hour == 0 && ce.StartsAtUtc.Minute == 0 && ce.StartsAtUtc.Second == 0 &&
                 ce.EndsAtUtc.Hour == 0 && ce.EndsAtUtc.Minute == 0 && ce.EndsAtUtc.Second == 0))
            .WithMessage("All-day events must start and end at midnight (00:00:00)");
    }
}
