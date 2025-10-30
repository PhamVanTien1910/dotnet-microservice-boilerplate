using FluentValidation;

namespace IAM.Application.Handlers.Users.Commands.BlacklistToken;

public class BlacklistTokenCommandValidator : AbstractValidator<BlacklistTokenCommand>
{
    public BlacklistTokenCommandValidator()
    {
        RuleFor(x => x.Jti)
            .NotEmpty()
            .WithMessage("Jti is required.");

        RuleFor(x => x.TokenExpiry)
            .Must(tokenExpiry => tokenExpiry == null || tokenExpiry > DateTime.UtcNow)
            .WithMessage("Token expiry must be in the future if provided.");
    }
}