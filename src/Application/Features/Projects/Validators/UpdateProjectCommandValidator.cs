using Application.Features.Projects.Commands;
using FluentValidation;

namespace Application.Features.Projects.Validators;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Valid project ID is required.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.");
    }
}
