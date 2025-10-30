using FluentValidation;
using System.Text.RegularExpressions;

namespace IAM.Application.Handlers.Users.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MaximumLength(254)
                .WithMessage("Email must not exceed 254 characters");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .MaximumLength(128)
                .WithMessage("Password must not exceed 128 characters")
                .Must(NotContainWhitespace)
                .WithMessage("Password cannot contain whitespace characters");
        }

        private static bool NotContainWhitespace(string? password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            return !password.Any(char.IsWhiteSpace);
        }
    }
}