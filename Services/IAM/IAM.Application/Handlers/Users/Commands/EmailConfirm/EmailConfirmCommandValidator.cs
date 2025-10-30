using FluentValidation;

namespace IAM.Application.Handlers.Users.Commands.EmailConfirm
{
    public class EmailConfirmCommandValidator : AbstractValidator<EmailConfirmCommand>
    {
        public EmailConfirmCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Email confirmation token is required")
                .MinimumLength(10)
                .WithMessage("Token appears to be invalid")
                .MaximumLength(100)
                .WithMessage("Token must not exceed 100 characters");
        }
    }
}
