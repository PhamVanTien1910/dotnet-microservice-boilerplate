using FluentValidation;
using System.Text.RegularExpressions;

namespace IAM.Application.Handlers.Users.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        private static readonly Regex PasswordPattern = new(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?"":|;'`~<>{}[\]\\\/+=_-]).{8,}$",
            RegexOptions.Compiled);

        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Reset token is required");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required")
                .MinimumLength(8)
                .WithMessage("New password must be at least 8 characters long")
                .MaximumLength(128)
                .WithMessage("New password must not exceed 128 characters")
                .Must(BeAValidPassword)
                .WithMessage("New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")
                .Must(NotContainWhitespace)
                .WithMessage("New password cannot contain whitespace characters");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required")
                .Equal(x => x.NewPassword)
                .WithMessage("Confirm password must match the new password");
        }

        private static bool BeAValidPassword(string? password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            return PasswordPattern.IsMatch(password);
        }

        private static bool NotContainWhitespace(string? password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            return !password.Any(char.IsWhiteSpace);
        }
    }
}
