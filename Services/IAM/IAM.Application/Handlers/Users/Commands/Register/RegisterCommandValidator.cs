using System.Text.RegularExpressions;
using FluentValidation;

namespace IAM.Application.Handlers.Users.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {

        private static readonly Regex PasswordPattern = new(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?"":|;'`~<>{}[\]\\\/+=_-]).{8,}$",
            RegexOptions.Compiled);

        private static readonly Regex NamePattern = new(
            @"^[a-zA-Z\s'-]+$",
            RegexOptions.Compiled);

        private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Admin", "User"
        };

        public RegisterCommandValidator()
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
                .Must(BeAValidPassword)
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")
                .Must(NotContainWhitespace)
                .WithMessage("Password cannot contain whitespace characters");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required")
                .Equal(x => x.Password)
                .WithMessage("Confirm password must match the password");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .MaximumLength(100)
                .WithMessage("First name must not exceed 100 characters")
                .Must(BeValidName)
                .WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .MaximumLength(100)
                .WithMessage("Last name must not exceed 100 characters")
                .Must(BeValidName)
                .WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role is required")
                .Must(BeValidRole)
                .WithMessage("Role must be either 'User' or 'Admin'");

            // Conditional phone number validation
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required");
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

        private static bool BeValidName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return NamePattern.IsMatch(name);
        }

        private static bool BeValidRole(string? role)
        {
            if (string.IsNullOrEmpty(role))
                return false;

            return ValidRoles.Contains(role);
        }
    }
}