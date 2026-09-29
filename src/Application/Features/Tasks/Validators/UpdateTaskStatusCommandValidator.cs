using Application.Features.Tasks.Commands;
using FluentValidation;

namespace Application.Features.Tasks.Validators;

public class UpdateTaskStatusCommandValidator : AbstractValidator<UpdateTaskStatusCommand>
{
    public UpdateTaskStatusCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Valid task ID is required.");

        RuleFor(v => v.Status)
            .IsInEnum().WithMessage("A valid task status is required.");
    }
}
