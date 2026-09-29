using Application.Features.Tasks.Commands;
using FluentValidation;

namespace Application.Features.Tasks.Validators;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(2000).WithMessage("Task description must not exceed 2000 characters.")
            .When(v => !string.IsNullOrEmpty(v.Description));

        RuleFor(v => v.ProjectId)
            .GreaterThan(0).WithMessage("Valid project ID is required.");

        RuleFor(v => v.Priority)
            .IsInEnum().WithMessage("A valid task priority is required.");

        RuleFor(v => v.Status)
            .Equal(TaskStatus.Todo).WithMessage("New tasks must start with 'Todo' status.");
    }
}
