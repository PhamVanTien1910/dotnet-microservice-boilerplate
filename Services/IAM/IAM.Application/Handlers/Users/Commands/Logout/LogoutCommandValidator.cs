using FluentValidation;

namespace IAM.Application.Handlers.Users.Commands.Logout
{
    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .MaximumLength(2048)
                .WithMessage("Refresh token must not exceed 2048 characters")
                .When(x => !string.IsNullOrEmpty(x.RefreshToken));
        }
    }
}
