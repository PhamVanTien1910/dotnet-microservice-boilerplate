using System.Net.Mail;
using BuildingBlocks.Domain.Exceptions;

namespace IAM.Domain.Aggregates.Users.ValueObjects;

public record Email
{
    public string Value { get; init; }

    private Email(string value) => Value = value;

    public static Email Create(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            throw new BadRequestException("Email cannot be empty.");
        }

        try
        {
            var normalizedEmail = new MailAddress(emailAddress).Address;
            return new Email(normalizedEmail);
        }
        catch (FormatException)
        {
            throw new BadRequestException("The email address format is invalid.");
        }
    }
}
