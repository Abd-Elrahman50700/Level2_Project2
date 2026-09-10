using Application.Features.Tasks.Commands;
using FluentValidation;

namespace Application.Features.Tasks.Validators;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Valid task ID is required.");

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

        RuleFor(v => v.ProjectId)
            .GreaterThan(0).WithMessage("Valid project ID is required.");
    }
}
