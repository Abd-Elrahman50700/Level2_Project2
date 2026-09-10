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

        RuleFor(v => v.ProjectId)
            .GreaterThan(0).WithMessage("Valid project ID is required.");
    }
}
