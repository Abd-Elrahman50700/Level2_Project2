using Application.Features.Comments.Commands;
using FluentValidation;

namespace Application.Features.Comments.Validators;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(v => v.TaskId)
            .GreaterThan(0).WithMessage("Valid task ID is required.");

        RuleFor(v => v.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(1000).WithMessage("Comment content must not exceed 1000 characters.");

        RuleFor(v => v.Author)
            .NotEmpty().WithMessage("Comment author is required.")
            .MaximumLength(100).WithMessage("Comment author must not exceed 100 characters.");
    }
}
