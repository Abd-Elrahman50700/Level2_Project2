using Application.Features.Projects.Commands;
using FluentValidation;

namespace Application.Features.Projects.Validators;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Project description must not exceed 1000 characters.")
            .When(v => !string.IsNullOrEmpty(v.Description));

        RuleFor(v => v.UserId)
            .MaximumLength(450).WithMessage("User ID must not exceed 450 characters.")
            .When(v => !string.IsNullOrEmpty(v.UserId));
    }
}
