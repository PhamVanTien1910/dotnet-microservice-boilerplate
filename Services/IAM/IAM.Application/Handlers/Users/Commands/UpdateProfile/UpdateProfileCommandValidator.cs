using System.Text.RegularExpressions;
using FluentValidation;

namespace IAM.Application.Handlers.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
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

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required");

            RuleFor(x => x.AvatarUrl)
                .MaximumLength(200).WithMessage("Avatar URL must not exceed 200 characters.");
        }

        private static readonly Regex NamePattern = new(
            @"^[a-zA-Z\s'-]+$",
            RegexOptions.Compiled);

        private static bool BeValidName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return NamePattern.IsMatch(name);
        }
    }
}