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
            .MaximumLength(2000).WithMessage("Comment content must not exceed 2000 characters.");

        RuleFor(v => v.Author)
            .MaximumLength(100).WithMessage("Comment author must not exceed 100 characters.")
            .When(v => !string.IsNullOrEmpty(v.Author));
    }
}
