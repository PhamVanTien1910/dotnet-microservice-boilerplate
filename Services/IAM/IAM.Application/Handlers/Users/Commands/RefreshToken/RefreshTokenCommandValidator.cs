using FluentValidation;

namespace IAM.Application.Handlers.Users.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .MaximumLength(2048)
                .WithMessage("Refresh token must not exceed 2048 characters")
                .When(x => !string.IsNullOrEmpty(x.RefreshToken))
                .NotEmpty().WithMessage("Refresh token is required");
        }
    }
}